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

namespace web.Controllers;

[ApiController]
[Route("api/upload")]
public class FileUploadApiController : ControllerBase
{
    private readonly ILogger<FileUploadApiController> _logger;
    private readonly ILogService _logService;
    private readonly IUploadProcessor _uploadProcessor;

    public FileUploadApiController(ILogger<FileUploadApiController> logger,
                                   ILogService logService,
                                   IUploadProcessor uploadProcessor)
    {
        _logger = logger;
        _logService = logService;
        _uploadProcessor = uploadProcessor;
    }

    [HttpPost("part")]
    [Authorize]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [DisableRequestSizeLimit]
    [DisableFormValueModelBinding]
    [IgnoreAntiforgeryToken]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    public async Task<IActionResult> UploadLargeFile()
    {
        if (Request.ContentType is null)
            return BadRequest();

        FilePartMetaData? fileMeta = null;

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

                    continue;
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
                    await _uploadProcessor.ProcessFilePart(filePart);

                }


                section = await reader.ReadNextSectionAsync();
            }

            return NoContent();

        }
        catch (EndOfStreamException ex)
        {
            _logger.LogWarning(ex, "Client sent incomplete file chunk for UID: {Uid}", fileMeta?.Uid);
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
        Debug.Assert(User.Identity?.Name is not null, "User identity should not be null in this context");

        if (!long.TryParse(User.FindFirst("Id")?.Value, out long user_id))
        {
            return BadRequest("Bad auth data");
        }

        var fileDto = new FileCreationDto(HttpUtility.HtmlEncode(requestModel.FileName),
                                          requestModel.FileSize,
                                          requestModel.TotalParts,
                                          requestModel.ExpectedPartSize,
                                          user_id);

        Result<string> result = _uploadProcessor.AddNewFileHandle(fileDto);

        switch (result)
        {
            case Success<string> success:
                _logger.LogInformation($"FileHandle opened(OK) Filename: {requestModel.FileName}, Filesize:{fileDto.FileSize}");
                return Ok(success.Value);

            case Failure<string> f:
                _logger.LogError("Failed to add new file handle", f.Error);
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

}
