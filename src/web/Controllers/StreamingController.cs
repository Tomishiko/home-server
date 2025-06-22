using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using web.Helpers;
using web.Services;
using System.Text.Json;
using web.Models;
using System.Web;
using web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using core.Services;
using core.Models;
using System.Diagnostics;

namespace web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private StreamedFileCompositor _fileCompositor;
    private FileMeta? fileMeta;
    private readonly ILogger<StreamingController> _logger;
    private readonly ILogService logService;
    public StreamingController(StreamedFileCompositor compositor, ILogger<StreamingController> logger, ILogService logService)
    {
        _fileCompositor = compositor;
        _logger = logger;
        this.logService = logService;
    }


    [HttpPost("uploadlarge")]
    [Authorize]
    [DisableFormValueModelBinding]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadLargeFileAsync()
    {
        if (Request.ContentType is null)
            return BadRequest();
        try
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                throw new FormatException("Form without multipart content.");
            }

            var subDirectory = string.Empty;
            var count = 0;
            ulong totalSize = 0;
            // find the boundary
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
            // use boundary to iterator through the multipart section
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();
            do
            {
                if (section is null)
                    break;

                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                    break;

                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition) && contentDisposition.Name == "meta")
                    {
                        var formData = await section.AsFormDataSection().GetValueAsync();

                        fileMeta = JsonSerializer.Deserialize<FileMeta>(formData);

                    }
                    section = await reader.ReadNextSectionAsync();
                    continue;
                }

                IStreamedFile file;

                if (_fileCompositor.StreamedFiles.TryGetValue(fileMeta.uid, out file))
                {
                    byte[] buffer = new byte[fileMeta.bytesRead]; //TODO: Consider using Span<> or Memory<>
                    //await section.AsFileSection().FileStream.ReadExactlyAsync(buffer, 0, fileMeta.bytesRead);
                    //
                    //Memory<byte> buffer = new Memory<byte>(new byte[fileMeta.bytesRead]);
                    //Span<byte> buffe = new Span<byte>(new byte[fileMeta.bytesRead]);
                    await section.AsFileSection().FileStream.ReadExactlyAsync(buffer);

                    await RandomAccess.WriteAsync(file.GetFileHandle, buffer, fileMeta.currentPart * file.PartSize);

                    file.IncrementPartsWrittenLocked();


                }
                section = await reader.ReadNextSectionAsync();
            } while (section != null);

            return Ok(new { Count = count, Size = Utility.BytesToStringOptimized(totalSize) });

        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while streaming file");
            return BadRequest($"Error: {exception.Message}");
        }
    }

    [HttpPost("handshake")]
    [Authorize]
    public IActionResult Handshake([FromBody] FileHandshake requestModel)
    {
        if (string.IsNullOrEmpty(requestModel.fileName) || requestModel.fileSize < 1 || requestModel.expectedPartSize <= 64)
        {

            _logger.LogWarning($"fname:{requestModel.fileName}, fsize:{requestModel.fileSize},expectedPartSize:{requestModel.expectedPartSize}");
            return BadRequest("Bad request data");
        }
        Debug.Assert(User.Identity?.Name is not null, "User identity should not be null in this context");
        try
        {
            var encodeFileName = HttpUtility.HtmlEncode(requestModel.fileName);
            var UniqueID = Guid.NewGuid().ToString();

            var fileHandle = System.IO.File.OpenHandle($"wwwroot/files/{UniqueID}",
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.Write,
                            preallocationSize: requestModel.fileSize);
            uint user_id;
            if (!uint.TryParse(User.FindFirst("Id")?.Value, out user_id))
            {
                return BadRequest("Bad auth data");
            }

            var streamedFile = new StreamedFile
            {
                Id = UniqueID,
                FileName = requestModel.fileName,
                PartSize = requestModel.expectedPartSize,
                FileSize = requestModel.fileSize,
                TotalFileParts = requestModel.totalParts,
                fileHandleProvider = new FileHandleProvider(fileHandle),
                Created = DateTime.Now,
                OwnerId = user_id
            };
            streamedFile.CloseEvent += _fileCompositor.CloseEventHandlerAsync;
            if (!_fileCompositor.StreamedFiles.TryAdd(UniqueID, streamedFile))
            {
                return StatusCode(500);
                // TODO: handle errors and existing uuids
            }
            _logger.LogInformation($"FileHandle opened(OK) Filename: {requestModel.fileName}, Filesize:{streamedFile.FileSize}");
            return Ok(UniqueID);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while handshake");
            return BadRequest($"Check the handshake data {ex.Message}");
        }




    }
    [HttpPost("abort")]
    [Authorize]
    public IActionResult AbortStreaming(string uid)
    {
        if (!_fileCompositor.StreamedFiles.TryGetValue(uid, out var file))
            return BadRequest("Identifier does not exist");

        file.Close();
        var fileInf = new FileInfo(Path.Combine("wwwroot", "files", file.FileName));
        fileInf.Delete();
        return Ok(file.FileName);

    }
    private class FileMeta
    {
        public string uid { get; set; } = string.Empty;
        public int currentPart { get; set; }
        public int bytesRead { get; set; }
    }
}
