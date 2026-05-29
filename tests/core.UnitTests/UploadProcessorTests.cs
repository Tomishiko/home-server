using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Linq.Expressions;
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
using Microsoft.IdentityModel.Abstractions;
using Moq;
using MockQueryable.Moq;
using Xunit;
using Microsoft.AspNetCore.WebUtilities;

namespace core.UnitTests;

public class UploadProcessorTests
{
    [Fact]
    public async Task AddNewFileHandleAsync_CreatesSessionAndReturnsHandshakeResponseData()
    {

        var mockWriter = new Mock<IPhysicalFileWriter>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();
        mockFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<long>())).Returns(mockWriter.Object);

        var stub = new List<FileUploadStateEntity>();
        var mockSet = stub.BuildMockDbSet();
        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.FileUploadState).Returns(mockSet.Object);


        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, new Mock<IServiceScopeFactory>().Object);
        var processor = new UploadProcessor(monitor, NullLogger<UploadProcessor>.Instance);

        var fileDto = new FileCreationDto("test.txt", 1024, 4, 256, 1, new byte[32]);
        var options = new FileUploadOptions { StoragePath = "/tmp" };

        var result = await processor.AddNewFileHandleAsync(fileDto, mockContext.Object, mockFactory.Object, options);

        Assert.IsType<Success<FileHandshakeResponseDto>>(result);
        if (result is Success<FileHandshakeResponseDto> success)
        {
            Assert.Equal(256, success.Value.PartSize);
            Assert.Equal(0, success.Value.PartsWritten);
            Assert.NotEmpty(success.Value.Uuid);
        }

        mockSet.Verify(d => d.Add(It.IsAny<FileUploadStateEntity>()), Times.Once);
        mockContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddNewFileHandleAsync_CreatesCorrectFileUploadStateEntity()
    {
        // Mocks
        var capturedEntity = (FileUploadStateEntity?)null;
        var mockDb = new Mock<IApplicationDbContext>();
        var stub = new List<FileUploadStateEntity>();
        var mockDbSet = stub.BuildMockDbSet();

        mockDb.Setup(d => d.FileUploadState).Returns(mockDbSet.Object);
        mockDbSet.Setup(s => s.Add(It.IsAny<FileUploadStateEntity>()))
            .Callback<FileUploadStateEntity>(stub.Add);

        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockWriter = new Mock<IPhysicalFileWriter>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();
        mockFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<long>())).Returns(mockWriter.Object);

        // Arrange
        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, new Mock<IServiceScopeFactory>().Object);
        var processor = new UploadProcessor(monitor, NullLogger<UploadProcessor>.Instance);

        var fingerprint = new byte[] { 1, 2, 3, 4, 5 };
        var fileDto = new FileCreationDto("myfile.bin", 2048, 8, 256, 99, fingerprint);
        var options = new FileUploadOptions { StoragePath = "/tmp" };

        // Act
        var result = await processor.AddNewFileHandleAsync(fileDto, mockDb.Object, mockFactory.Object, options);

        //Assert
        Assert.IsType<Success<FileHandshakeResponseDto>>(result);
        var responseDto = (result as Success<FileHandshakeResponseDto>).Value;

        var session = monitor.ActiveSessions[new Guid(responseDto.Uuid)];
        capturedEntity = stub.FirstOrDefault();

        Assert.NotNull(result);
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
    public async Task AddNewFileHandleAsync_ReturnsExistingUuidIfSessionAlreadyExist()
    {

        var mockDb = new Mock<IApplicationDbContext>();

        var fingerprint = new byte[] { 1, 2, 3, 4, 5 };
        var fileDto = new FileCreationDto("myfile.bin", 2048, 8, 256, 99, fingerprint);
        var options = new FileUploadOptions { StoragePath = "/tmp" };
        var fileGuid = Guid.NewGuid();
        var stubEntity = new FileUploadStateEntity
        {
            PartsWritten = 0,
            Fingerprint = fileDto.Fingerprint,
            Id = fileGuid,
            Metadata = new FileWriterMeta(0, 0, "", 0, 0),
            PartsBitfield = 0
        };
        var stub = new List<FileUploadStateEntity> { stubEntity };
        var mockDbSet = stub.BuildMockDbSet();

        mockDb.Setup(d => d.FileUploadState).Returns(mockDbSet.Object);

        var mockWriter = new Mock<IPhysicalFileWriter>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();
        mockFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<long>())).Returns(mockWriter.Object);

        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, new Mock<IServiceScopeFactory>().Object);
        var processor = new UploadProcessor(monitor, NullLogger<UploadProcessor>.Instance);


        // Act
        var result = await processor.AddNewFileHandleAsync(fileDto, mockDb.Object, mockFactory.Object, options);
        Assert.IsType<Success<FileHandshakeResponseDto>>(result);
        var responseDto = (result as Success<FileHandshakeResponseDto>).Value;

        Assert.Equal(stubEntity.PartsWritten, responseDto.PartsWritten);
        Assert.Equal(stubEntity.Id.ToString(), responseDto.Uuid);
    }



}
