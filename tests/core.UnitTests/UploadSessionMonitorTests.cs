using System;
using System.Threading.Tasks;
using core.Domain;
using core.Interfaces;
using core.Models;
using core.Services;
using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace core.UnitTests;

public class UploadSessionMonitorTests
{
    [Fact]
    public async Task OnCloseEventAsync_HappyPath_RemovesSessionAndStagesFile()
    {
        var mockFileService = new Mock<IFileService>();
        var mockLogService = new Mock<ILogService>();
        var mockDb = new Mock<IApplicationDbContext>();
        var mockDbSet = new Mock<DbSet<FileUploadStateEntity>>();

        mockDb.Setup(d => d.FileUploadState).Returns(mockDbSet.Object);
        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(ILogService))).Returns(mockLogService.Object);
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(IFileService))).Returns(mockFileService.Object);
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(IApplicationDbContext))).Returns(mockDb.Object);

        var mockFactory = new Mock<IServiceScopeFactory>();
        mockFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, mockFactory.Object);

        var mockWriter = new Mock<IPhysicalFileWriter>();
        mockWriter.Setup(w => w.IsClosed).Returns(false);

        var randomStr = Generators.RandomString32();

        var fileState = new UploadingFileState(
            new FileCreationDto("document.pdf", 1024, 1, 1024, 5, randomStr, true),
            "/tmp",
            Guid.NewGuid(),
            new TestPhysicalFileWriterFactory(mockWriter.Object));

        var fileId = fileState.Uuid;
        monitor.ActiveSessions.TryAdd(fileId, fileState);

        var eventArgs = new CloseFileEventArgs(fileId, "document.pdf", 1024, DateTime.UtcNow);
        monitor.OnCloseEventAsync(null, eventArgs);

        await Task.Delay(100);

        Assert.False(monitor.ActiveSessions.ContainsKey(fileId));
        mockFileService.Verify(fs => fs.StageNewFileRecord(
            fileId.ToString(),
            ".pdf",
            "document",
            1024,
            5,
            true), Times.Once);
    }


    [Fact]
    public async Task OnCloseEventAsync_WithoutExtension_StagesFileWithEmptyExtension()
    {
        var mockFileService = new Mock<IFileService>();
        var mockLogService = new Mock<ILogService>();
        var mockDb = new Mock<IApplicationDbContext>();
        var mockDbSet = new Mock<DbSet<FileUploadStateEntity>>();

        mockDb.Setup(d => d.FileUploadState).Returns(mockDbSet.Object);
        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(ILogService))).Returns(mockLogService.Object);
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(IFileService))).Returns(mockFileService.Object);
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(IApplicationDbContext))).Returns(mockDb.Object);

        var mockFactory = new Mock<IServiceScopeFactory>();
        mockFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, mockFactory.Object);

        var mockWriter = new Mock<IPhysicalFileWriter>();
        mockWriter.Setup(w => w.IsClosed).Returns(false);

        var randomStr = Generators.RandomString32();
        var fileState = new UploadingFileState(
            new FileCreationDto("noextension", 512, 1, 512, 3, randomStr, true),
            "/tmp",
            Guid.NewGuid(),
            new TestPhysicalFileWriterFactory(mockWriter.Object));

        var fileId = fileState.Uuid;
        monitor.ActiveSessions.TryAdd(fileId, fileState);

        var eventArgs = new CloseFileEventArgs(fileId, "noextension", 512, DateTime.UtcNow);
        monitor.OnCloseEventAsync(null, eventArgs);

        await Task.Delay(100);

        mockFileService.Verify(fs => fs.StageNewFileRecord(
            fileId.ToString(),
            string.Empty,
            "noextension",
            512,
            3,
            true), Times.Once);
    }

    [Fact]
    public async Task OnCloseEventAsync_TryRemoveFails_LogsErrorAndDoesNotStageFile()
    {
        var mockFileService = new Mock<IFileService>();
        var mockLogService = new Mock<ILogService>();
        var mockDb = new Mock<IApplicationDbContext>();

        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(ILogService))).Returns(mockLogService.Object);
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(IFileService))).Returns(mockFileService.Object);
        mockServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(IApplicationDbContext))).Returns(mockDb.Object);

        var mockFactory = new Mock<IServiceScopeFactory>();
        mockFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        var monitor = new UploadSessionMonitor(NullLogger<UploadSessionMonitor>.Instance, mockFactory.Object);

        var nonExistentFileId = Guid.NewGuid();
        var eventArgs = new CloseFileEventArgs(nonExistentFileId, "missing.txt", 100, DateTime.UtcNow);

        monitor.OnCloseEventAsync(null, eventArgs);

        await Task.Delay(100);

        mockLogService.Verify(ls => ls.AddNewLog(It.IsAny<LogDto>()), Times.Once);
        mockFileService.Verify(fs => fs.StageNewFileRecord(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<bool>()), Times.Never);
    }

    private sealed class TestPhysicalFileWriter : IPhysicalFileWriter
    {
        public bool IsClosed { get; set; }

        public ValueTask Write(ReadOnlyMemory<byte> blob, long offset, System.Threading.CancellationToken ct)
        {
            return ValueTask.CompletedTask;
        }

        public void FlushToDisk()
        {
        }

        public void Dispose()
        {
            IsClosed = true;
        }
    }

    private sealed class TestPhysicalFileWriterFactory : IPhysicalFileWriterFactory
    {
        private readonly IPhysicalFileWriter _writer;

        public TestPhysicalFileWriterFactory(IPhysicalFileWriter writer)
        {
            _writer = writer;
        }

        public IPhysicalFileWriter Create(string filePath, long preallocationSize)
        {
            return _writer;
        }
    }
}
