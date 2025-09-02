using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using web.Helpers;
using web.Services;
using System.Text.Json;
using web.Models;
using System.Buffers;
using System.Web;
using web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using core.Services;
using System.Diagnostics;

namespace web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private StreamedFileCompositor _fileCompositor;
    private FileMeta? _fileMeta;
    private readonly ILogger<StreamingController> _logger;
    private readonly ILogService _logService;

    public StreamingController(StreamedFileCompositor compositor, ILogger<StreamingController> logger, ILogService logService)
    {
        _fileCompositor = compositor;
        _logger = logger;
        _logService = logService;
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
                return BadRequest("Form without multipart content.");
            }

            var subDirectory = string.Empty;
            var count = 0;
            ulong totalSize = 0;
            // find the boundary
            string boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
            // use boundary to iterator through the multipart section
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            MultipartSection? section = await reader.ReadNextSectionAsync();
            do
            {
                if (section is null)
                    break;

                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition)) break;

                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition) && contentDisposition.Name == "meta")
                    {

                        // there is no way we have null values propagated in this block.
                        // all nulls returned with BadRequest
#pragma warning disable 8602, 8600

                        string formData = await section.AsFormDataSection()
                                                    .GetValueAsync();
                        if (string.IsNullOrEmpty(formData)) return BadRequest();

                        _fileMeta = JsonSerializer.Deserialize<FileMeta>(formData);
                        if (_fileMeta is null) return BadRequest();

                    }
                    section = await reader.ReadNextSectionAsync();
                    continue;
                }

                IStreamedFile file;

                if (_fileCompositor.StreamedFiles.TryGetValue(_fileMeta.uid, out file))
                {
                    using var owner = MemoryPool<byte>.Shared.Rent(_fileMeta.bytesRead);
                    Memory<byte> buffer = owner.Memory;

                    await section.AsFileSection().FileStream
                                 .ReadExactlyAsync(buffer.Slice(0, _fileMeta.bytesRead));
                    await RandomAccess.WriteAsync(file.GetFileHandle,
                             buffer.Slice(0, _fileMeta.bytesRead),
                            _fileMeta.currentPart * file.PartSize);

                    file.IncrementPartsWrittenLocked();


                }

#pragma warning restore 8602,8600

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
        if (string.IsNullOrEmpty(requestModel.fileName)
            || requestModel.fileSize < 1
            || requestModel.expectedPartSize <= 64)
        {

            _logger.LogWarning($"fname:{requestModel.fileName}, fsize:{requestModel.fileSize},expectedPartSize:{requestModel.expectedPartSize}");
            return BadRequest("Bad request data");
        }
        Debug.Assert(User.Identity?.Name is not null, "User identity should not be null in this context");
        try
        {
            string? encodeFileName = HttpUtility.HtmlEncode(requestModel.fileName);
            string UniqueID = Guid.NewGuid().ToString();
            uint user_id;
            if (!uint.TryParse(User.FindFirst("Id")?.Value, out user_id))
            {
                return BadRequest("Bad auth data");
            }

            FileHandleConfig config = new($"wwwroot/files/{UniqueID}",
                                         FileMode.CreateNew,
                                         FileAccess.Write,
                                         FileShare.Write,
                                         requestModel.fileSize);

            StreamedFile streamedFile = new(config,
                                        UniqueID,
                                        requestModel.totalParts,
                                        requestModel.fileName,
                                        requestModel.expectedPartSize,
                                        user_id);

            streamedFile.CloseEvent += _fileCompositor.OnCloseEventAsync;
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
            return BadRequest($"Something unexpected happend while performing handshake {ex.Message}");
        }




    }
    [HttpPost("abort")]
    [Authorize]
    public IActionResult AbortStreaming(string uid)
    {
        if (!_fileCompositor.StreamedFiles.TryGetValue(uid, out var file))
            return BadRequest("Identifier does not exist");

        file.Dispose();
        var fileInf = new FileInfo(Path.Combine("wwwroot", "files", file.FileName));
        fileInf.Delete();
        return Ok(file.FileName);

    }
    private sealed class FileMeta
    {
        public string uid { get; set; } = string.Empty;
        public int currentPart { get; set; }
        public int bytesRead { get; set; }
    }
}
