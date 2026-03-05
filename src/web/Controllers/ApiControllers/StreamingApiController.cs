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
using System.IO.Pipelines;

namespace web.Controllers;

[ApiController]
[Route("api/streaming")]
public class StreamingApiController : ControllerBase
{
    private readonly StreamedFileCompositor _fileCompositor;
    private readonly ILogger<StreamingApiController> _logger;
    private readonly ILogService _logService;
    private const long maxPartSize = 1024 * 1024 * 10;//10 Mb

    public StreamingApiController(StreamedFileCompositor compositor,
                               ILogger<StreamingApiController> logger,
                               ILogService logService)
    {
        _fileCompositor = compositor;
        _logger = logger;
        _logService = logService;
    }

    [HttpPost("uploadlarge")]
    [Authorize]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [DisableRequestSizeLimit]
    [DisableFormValueModelBinding]
    [IgnoreAntiforgeryToken]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    public async Task<IActionResult> UploadLargeFileAsync()
    {
        if (Request.ContentType is null)
            return BadRequest();

        FilePartMeta? fileMeta = null;
        var subDirectory = string.Empty;
        var count = 0;
        long totalSize = 0;
        try
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest("Wrong contetnt type");
            }

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
                    out var contentDisposition))
                {
                    break;
                }

                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition)
                            && contentDisposition.Name == "meta")
                    {

                        string formData = await section.AsFormDataSection()
                                                    .GetValueAsync();
                        if (string.IsNullOrEmpty(formData)) return BadRequest();

                        fileMeta = JsonSerializer.Deserialize<FilePartMeta>(formData);

                        if (fileMeta is null) return BadRequest();

                    }
                    section = await reader.ReadNextSectionAsync();
                    continue;
                }

                IStreamedFile? file;

                if (fileMeta is not null &&
                        _fileCompositor.StreamedFiles.TryGetValue(fileMeta.uid, out file))
                {
                    if (fileMeta.bytesRead >= maxPartSize)
                    {
                        BadRequest("File part is too bid");
                    }

                    using var owner = MemoryPool<byte>.Shared.Rent(fileMeta.bytesRead);
                    Memory<byte> buffer = owner.Memory[..fileMeta.bytesRead];
                    Stream? fileStream = section.AsFileSection()?.FileStream;

                    if (fileStream is not null) //TODO: try pipe reader
                    {
                        await fileStream.ReadExactlyAsync(buffer);

                        await RandomAccess.WriteAsync(file.GetFileHandle,
                                 buffer.Slice(0, fileMeta.bytesRead),
                                fileMeta.currentPart * file.PartSize);

                        file.IncrementPartsWrittenLocked();


                    }

                }


                section = await reader.ReadNextSectionAsync();
            } while (section != null);

            return Ok(new { Count = count, Size = Utility.BytesToStringOptimized(totalSize) });

        }
        catch (EndOfStreamException ex)
        {
            _logger.LogWarning(ex, "Client sent incomplete file chunk for UID: {Uid}", fileMeta?.uid);
            return BadRequest("The uploaded file chunk was truncated or incomplete.");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Network I/O error occurred during stream reading.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while streaming file");
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
            long user_id;
            if (!long.TryParse(User.FindFirst("Id")?.Value, out user_id))
            {
                foreach (var claim in User.Claims)
                {
                    _logger.LogDebug($"{claim.ValueType}:{claim.Value}");
                }
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

    private sealed class FilePartMeta
    {
        public string uid { get; set; } = string.Empty;
        public int currentPart { get; set; }
        public int bytesRead { get; set; }
    }
}
