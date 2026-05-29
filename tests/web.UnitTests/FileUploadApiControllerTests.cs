using System;
using System.IO.Pipelines;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using core.Interfaces;
using core.Models;
using core.Models.Generic;
using core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using web.Controllers;
using web.Models;
using Xunit;

namespace web.UnitTests;

public class FileUploadApiControllerTests
{
    [Fact]
    public async Task UploadLargeFile_ValidUploadPart_ReturnsOkWithUploadPartSuccess()
    {
        var mockLogger = new Mock<ILogger<FileUploadApiController>>();
        var mockProcessor = new Mock<IUploadProcessor>();

        var uploadSuccess = new UploadPartSuccess(0, 1024);
        mockProcessor.Setup(p => p.ProcessFilePartPipe(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<PipeReader>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<UploadPartSuccess>)uploadSuccess);

        var controller = new FileUploadApiController(mockLogger.Object, mockProcessor.Object);

        // Mock HttpContext and BodyReader
        var mockContext = new Mock<HttpContext>();
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(new byte[] { 1, 2, 3, 4 });
        await pipe.Writer.CompleteAsync();

        mockContext.Setup(c => c.Request.BodyReader).Returns(pipe.Reader);
        controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

        var result = await controller.UploadLargeFile(Guid.NewGuid(), 0, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<UploadPartSuccess>(okResult.Value);
    }

    [Fact]
    public async Task UploadLargeFile_ProcessingFails_ReturnsStatusCodeWithErrorMessage()
    {
        var mockLogger = new Mock<ILogger<FileUploadApiController>>();
        var mockProcessor = new Mock<IUploadProcessor>();

        var error = new Error("Upload failed", 400);
        mockProcessor.Setup(p => p.ProcessFilePartPipe(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<PipeReader>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<UploadPartSuccess>)error);

        var controller = new FileUploadApiController(mockLogger.Object, mockProcessor.Object);

        var mockContext = new Mock<HttpContext>();
        var pipe = new Pipe();
        await pipe.Writer.CompleteAsync();

        mockContext.Setup(c => c.Request.BodyReader).Returns(pipe.Reader);
        controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

        var result = await controller.UploadLargeFile(Guid.NewGuid(), 0, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, statusResult.StatusCode);
    }

    [Fact]
    public async Task Handshake_ValidRequestWithAuthorizedUser_ReturnsCreatedWithHandshake()
    {
        var mockLogger = new Mock<ILogger<FileUploadApiController>>();
        var mockProcessor = new Mock<IUploadProcessor>();
        var mockDb = new Mock<IApplicationDbContext>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();

        var handshake = new FileHandshakeResponseDto(Guid.NewGuid().ToString(), 256,0);

        mockProcessor.Setup(p => p.AddNewFileHandleAsync(
            It.IsAny<FileCreationDto>(),
            It.IsAny<IApplicationDbContext>(),
            It.IsAny<IPhysicalFileWriterFactory>(),
            It.IsAny<FileUploadOptions>()))
            .ReturnsAsync((Result<FileHandshakeResponseDto>)handshake);

        var controller = new FileUploadApiController(mockLogger.Object, mockProcessor.Object);

        var mockContext = new Mock<HttpContext>();
        var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    new Claim(AppClaimTypes.Identity, "123"),
                    new Claim(AppClaimTypes.Name, "Test")
                ], "test", AppClaimTypes.Name, AppClaimTypes.Role));

        mockContext.Setup(c => c.User).Returns(user);

        controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

        var fingerprintStub = new byte[32];
        Random.Shared.NextBytes(fingerprintStub);

        var request = new FileHandshake { FileName = "test.txt", FileSize = 1024, FileFingerprint = fingerprintStub };
        var options = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptions { StoragePath = "/tmp" });
        var clientOptions = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptionsClient { PartSize = 256 });

        var result = await controller.Handshake(request, mockDb.Object, clientOptions, options, mockFactory.Object);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FileUploadApiController.UploadLargeFile), createdResult.ActionName);
        Assert.IsType<FileHandshakeResponseDto>(createdResult.Value);
    }

    [Fact]
    public async Task Handshake_MissingAuthClaim_ReturnsBadRequest()
    {
        var mockLogger = new Mock<ILogger<FileUploadApiController>>();
        var mockProcessor = new Mock<IUploadProcessor>();
        var stabUUid = Guid.NewGuid();

        mockProcessor.Setup(p => p.AddNewFileHandleAsync(It.IsAny<FileCreationDto>(),
                                                       It.IsAny<IApplicationDbContext>(),
                                                       It.IsAny<IPhysicalFileWriterFactory>(),
                                                       It.IsAny<FileUploadOptions>()))
                     .ReturnsAsync(new FileHandshakeResponseDto(stabUUid.ToString(), 1024,0));

        var mockDb = new Mock<IApplicationDbContext>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();

        var controller = new FileUploadApiController(mockLogger.Object, mockProcessor.Object);

        var mockContext = new Mock<HttpContext>();
        var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    //new Claim(AppClaimTypes.Identity,"1"),
                    //new Claim(AppClaimTypes.Name,"test")
                ], "test", AppClaimTypes.Name, AppClaimTypes.Role));
        mockContext.Setup(c => c.User).Returns(user);

        controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

        byte[] fingerprintStub = new byte[32];
        Random.Shared.NextBytes(fingerprintStub);
        var request = new FileHandshake { FileName = "test.txt", FileSize = 1024, FileFingerprint = fingerprintStub };
        var options = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptions { StoragePath = "/tmp" });
        var clientOptions = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptionsClient { PartSize = 256 });

        var result = await controller.Handshake(request, mockDb.Object, clientOptions, options, mockFactory.Object);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Handshake_InvalidAuthClaim_ReturnsBadRequest()
    {
        var mockLogger = new Mock<ILogger<FileUploadApiController>>();
        var mockProcessor = new Mock<IUploadProcessor>();
        var mockDb = new Mock<IApplicationDbContext>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();

        var controller = new FileUploadApiController(mockLogger.Object, mockProcessor.Object);

        var mockContext = new Mock<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AppClaimTypes.Identity, "not-a-number"),
            new Claim(AppClaimTypes.Name,"test")
        ], "test", AppClaimTypes.Name, AppClaimTypes.Role));
        mockContext.Setup(c => c.User).Returns(user);

        controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

        var fingerprintStub = new byte[32];
        Random.Shared.NextBytes(fingerprintStub);
        var request = new FileHandshake { FileName = "test.txt", FileSize = 1024, FileFingerprint = fingerprintStub };
        var options = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptions { StoragePath = "/tmp" });
        var clientOptions = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptionsClient { PartSize = 256 });

        var result = await controller.Handshake(request, mockDb.Object, clientOptions, options, mockFactory.Object);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Handshake_ProcessorReturnsFailure_ReturnsStatusCodeFromError()
    {
        var mockLogger = new Mock<ILogger<FileUploadApiController>>();
        var mockProcessor = new Mock<IUploadProcessor>();
        var mockDb = new Mock<IApplicationDbContext>();
        var mockFactory = new Mock<IPhysicalFileWriterFactory>();

        var error = new Error("Collision detected", 500);
        mockProcessor.Setup(p => p.AddNewFileHandleAsync(
            It.IsAny<FileCreationDto>(),
            It.IsAny<IApplicationDbContext>(),
            It.IsAny<IPhysicalFileWriterFactory>(),
            It.IsAny<FileUploadOptions>()))
            .ReturnsAsync((Result<FileHandshakeResponseDto>)error);

        var controller = new FileUploadApiController(mockLogger.Object, mockProcessor.Object);

        var mockContext = new Mock<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AppClaimTypes.Identity, "123"),
            new Claim(AppClaimTypes.Name,"test")
        ], "test", AppClaimTypes.Name, AppClaimTypes.Role));

        mockContext.Setup(c => c.User).Returns(user);

        controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };
        var fingerprintStub = new byte[32];
        Random.Shared.NextBytes(fingerprintStub);

        var request = new FileHandshake { FileName = "test.txt", FileSize = 1024, FileFingerprint = fingerprintStub };
        var options = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptions { StoragePath = "/tmp" });
        var clientOptions = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptionsClient { PartSize = 256 });

        var result = await controller.Handshake(request, mockDb.Object, clientOptions, options, mockFactory.Object);

        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
