using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using core.Domain;
using core.Interfaces;
using core.Models;
using core.Models.Generic;
using core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace core.UnitTests;

public class UploadProcessorTests
{
    [Fact]
    public async Task AddNewFileHandleAsync_CreatesSessionAndReturnsHandshakeWithPartSize()
    {
        var mockDb = new Mock<IApplicationDbContext>();
        var mockDbSet = new Mock<DbSet<FileUploadStateEntity>>();
        mockDb.Setup(d => d.FileUploadState).Returns(mockDbSet.Object);
        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockWriter = new Mock<IPhysicalFileWriter>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();
        mockFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<long>())).Returns(mockWriter.Object);

        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, new Mock<IServiceScopeFactory>().Object);
        var processor = new UploadProcessor(monitor, NullLogger<UploadProcessor>.Instance);

        var fileDto = new FileCreationDto("test.txt", 1024, 4, 256, 1, new byte[32]);
        var options = new FileUploadOptions { StoragePath = "/tmp" };

        var result = await processor.AddNewFileHandleAsync(fileDto, mockDb.Object, mockFactory.Object, options);

        Assert.IsType<Success<FileHandshakeResponseDto>>(result);
        if (result is Success<FileHandshakeResponseDto> success)
        {
            Assert.Equal(256, success.Value.PartSize);
            Assert.NotEmpty(success.Value.Uuid);
        }

        mockDbSet.Verify(set => set.Add(It.IsAny<FileUploadStateEntity>()), Times.Once);
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddNewFileHandleAsync_CreatesCorrectFileUploadStateEntity()
    {
        var capturedEntity = (FileUploadStateEntity?)null;
        var mockDb = new Mock<IApplicationDbContext>();
        var mockDbSet = new Mock<DbSet<FileUploadStateEntity>>();

        mockDbSet.Setup(set => set.Add(It.IsAny<FileUploadStateEntity>()))
            .Callback<FileUploadStateEntity>(e => capturedEntity = e);

        mockDb.Setup(d => d.FileUploadState).Returns(mockDbSet.Object);
        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockWriter = new Mock<IPhysicalFileWriter>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();
        mockFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<long>())).Returns(mockWriter.Object);

        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, new Mock<IServiceScopeFactory>().Object);
        var processor = new UploadProcessor(monitor, NullLogger<UploadProcessor>.Instance);

        var fingerprint = new byte[] { 1, 2, 3, 4, 5 };
        var fileDto = new FileCreationDto("myfile.bin", 2048, 8, 256, 99, fingerprint);
        var options = new FileUploadOptions { StoragePath = "/tmp" };

        var result = await processor.AddNewFileHandleAsync(fileDto, mockDb.Object, mockFactory.Object, options);
        var session = monitor.ActiveSessions[capturedEntity.Id];

        Assert.NotNull(result);
        Assert.IsType<Success<FileHandshakeResponseDto>>(result);
        Assert.NotNull(capturedEntity);

        Assert.Equal(fingerprint, capturedEntity.Fingerprint);
        Assert.Equal(2048, capturedEntity.Metadata.FileSize);
        Assert.Equal(8, capturedEntity.Metadata.TotalFileParts);
        Assert.Equal(256, capturedEntity.Metadata.PartSize);
        Assert.Equal("myfile.bin", capturedEntity.Metadata.FileName);
        Assert.Equal(99, capturedEntity.Metadata.OwnerId);


        Assert.Equal(session.FileFingerprint, capturedEntity.Fingerprint);
        Assert.Equal(session.FileSize, capturedEntity.Metadata.FileSize);
        Assert.Equal(session.PartSize, capturedEntity.Metadata.PartSize);
        Assert.Equal(session.FileName, capturedEntity.Metadata.FileName);
        Assert.Equal(session.OwnerId, capturedEntity.Metadata.OwnerId);
    }

    [Fact]
    public async Task ProcessFilePartPipe_ReturnsErrorWhenSessionNotFound()
    {
        var mockDb = new Mock<IApplicationDbContext>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();
        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, new Mock<IServiceScopeFactory>().Object);
        var processor = new UploadProcessor(monitor, NullLogger<UploadProcessor>.Instance);

        var fakeGuid = Guid.NewGuid();
        var pipe = new Pipe();
        await pipe.Writer.CompleteAsync();

        var result = await processor.ProcessFilePartPipe(fakeGuid, 0, pipe.Reader, CancellationToken.None);

        Assert.IsType<Failure<UploadPartSuccess>>(result);
    }

    [Fact]
    public async Task ProcessFilePartPipe_UsesCorrectSessionWhenExists()
    {
        var mockDb = new Mock<IApplicationDbContext>();
        var mockDbSet = new Mock<DbSet<FileUploadStateEntity>>();
        mockDb.Setup(d => d.FileUploadState).Returns(mockDbSet.Object);
        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockWriter = new Mock<IPhysicalFileWriter>();
        mockWriter.Setup(w => w.Write(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        mockWriter.Setup(w => w.IsClosed).Returns(false);

        var mockFactory = new Mock<IPhysicalFileWriterFactory>();
        mockFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<long>())).Returns(mockWriter.Object);

        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, new Mock<IServiceScopeFactory>().Object);
        var processor = new UploadProcessor(monitor, NullLogger<UploadProcessor>.Instance);

        var fileDto = new FileCreationDto("test.txt", 512, 2, 256, 1, new byte[32]);
        var options = new FileUploadOptions { StoragePath = "/tmp" };

        await processor.AddNewFileHandleAsync(fileDto, mockDb.Object, mockFactory.Object, options);

        var sessionId = (Guid?)null;
        foreach (var kvp in monitor.ActiveSessions)
        {
            sessionId = kvp.Key;
            break;
        }

        Assert.NotNull(sessionId);

        var payload = new byte[] { 1, 2, 3, 4 };
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(payload);
        await pipe.Writer.CompleteAsync();

        var result = await processor.ProcessFilePartPipe(sessionId.Value, 0, pipe.Reader, CancellationToken.None);

        Assert.IsType<Success<UploadPartSuccess>>(result);
    }
}
