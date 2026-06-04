using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using web.Helpers;
using System.Text.Json;
using web.Models;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using core.Services;
using core.Models;
using core.Models.Generic;
using core.utils.extensions;
using core.Interfaces;
using core.Domain;
using Microsoft.Extensions.Options;

namespace web.Controllers;

[ApiController]
[Route("api/upload")]
public class FileUploadApiController : ControllerBase
{
    private readonly ILogger<FileUploadApiController> _logger;
    private readonly IUploadProcessor _uploadProcessor;

    public FileUploadApiController(ILogger<FileUploadApiController> logger,
                                   IUploadProcessor uploadProcessor)
    {
        _logger = logger;
        _uploadProcessor = uploadProcessor;
    }


    [HttpPost("part/{uuid}")]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [DisableRequestSizeLimit]
    [DisableFormValueModelBinding]
    [IgnoreAntiforgeryToken]
    [Consumes(MediaTypeNames.Application.OctetStream)]
    public async Task<IActionResult> UploadLargeFile(Guid uuid,
                                                     [FromHeader(Name = "X-Part")] int currentPart,
                                                     CancellationToken ct)
    {

        Result<UploadPartSuccess> result = await _uploadProcessor.ProcessFilePartPipe(uuid,
                                                                   currentPart,
                                                                   HttpContext.Request.BodyReader, ct);
        switch (result)
        {
            case Success<UploadPartSuccess> s:
                return Ok(s.Value);

            case Failure<UploadPartSuccess> f:
                var statusCode = f.Error.Code ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, f.Error.Message);

            default: throw new Exception("Something went wrong while processing file_part");
        }
    }
    ///
    ///We pass DI args this way here to avoid as much allocations and unimportant
    ///logic on hot path as possible
    [HttpPost("handshake")]
    [Authorize]
    public async Task<IActionResult> Handshake([FromBody] FileHandshake requestModel,
                                               [FromServices] IApplicationDbContext db,
                                               [FromServices] IOptions<FileUploadOptionsClient> clientOptions,
                                               [FromServices] IOptions<FileUploadOptions> fileUploadOptions,
                                               [FromServices] IPhysicalFileWriterFactory fileWriterFactory)
    {

        if (!long.TryParse(User.FindFirst("Id")?.Value, out long user_id))
        {
            return BadRequest("Bad auth data");
        }
        var partSize = clientOptions.Value.PartSize;
        var totalParts = (requestModel.FileSize + partSize - 1) / partSize;

        var fileDto = new FileCreationDto(HttpUtility.HtmlEncode(requestModel.FileName),
                                          requestModel.FileSize,
                                          totalParts,
                                          partSize,
                                          user_id,
                                          requestModel.FileFingerprint.ToLower());

        Result<FileHandshakeResponseDto> result = await _uploadProcessor.AddNewFileHandleAsync(
                fileDto, db, fileWriterFactory, fileUploadOptions.Value);

        switch (result)
        {
            case Success<FileHandshakeResponseDto> success:
                _logger.LogInformation($"FileHandle opened(OK) Filename: {requestModel.FileName}, Filesize:{fileDto.FileSize}");

                return CreatedAtAction(nameof(UploadLargeFile),
                        success.Value.Uuid.ToString(),
                        success.Value);

            case Failure<FileHandshakeResponseDto> f:
                _logger.LogError("Failed to add new file handle {Error}", f.Error);
                return StatusCode(f.Error.Code ?? StatusCodes.Status400BadRequest);

            default: return StatusCode(500);
        }

    }

    [HttpPost("abort")]
    [Authorize]
    public IActionResult AbortStreaming(string uid)
    {

        throw new NotImplementedException();
    }

    [HttpPost("part")]
    [Authorize]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [DisableRequestSizeLimit]
    [DisableFormValueModelBinding]
    [IgnoreAntiforgeryToken]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    /// <summary>
    /// Deprecated, use <c cref="UploadLargeFile(Guid, int, CancellationToken)">UploadLargeFile</c> instead
    /// </summary>
    public async Task<IActionResult> UploadLargeFile_Old()
    {
        return NotFound("Deprecated");
        if (Request.ContentType is null)
            return BadRequest();

        FilePartMetaData? fileMeta = null;

        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            return BadRequest("Wrong contetnt type");
        }

        // find the boundary
        string boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
        // use boundary to iterator through the multipart section
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        MultipartSection? section = await reader.ReadNextSectionAsync();
        while (section != null)
        {

            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                out var contentDisposition))
            {
                break;
            }

            if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition)
                    && contentDisposition.Name == "meta")
            {
                if (section.AsFormDataSection() is not { } formDataSection)
                {
                    return BadRequest("Invalid form data section.");
                }

                string? formData = await formDataSection.GetValueAsync();
                if (formData.IsNullOrEmpty())
                {
                    return BadRequest("Metadata content is empty");
                }

                fileMeta = JsonSerializer.Deserialize<FilePartMetaData>(formData);
                if (fileMeta is null)
                {
                    return BadRequest("Failed to deserialize metadata");
                }
            }

            if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            {

                if (fileMeta is null)
                {
                    return BadRequest("Metadata section must precede file section");
                }
                if (fileMeta.BytesRead >= Utility.maxPartSize)
                {
                    BadRequest("Bad file part size");
                }

                if (section.AsFileSection() is not { FileStream: not null } fileSection)
                {
                    return BadRequest("Invalid or missing file stream.");
                }
                var filePart = new FilePartDto(fileMeta.Uid,
                                               fileMeta.CurrentPart,
                                               fileMeta.BytesRead,
                                               fileSection.FileStream);
                //await _uploadProcessor.ProcessFilePart(filePart);

            }


            section = await reader.ReadNextSectionAsync();
        }

        return NoContent();

    }
}
