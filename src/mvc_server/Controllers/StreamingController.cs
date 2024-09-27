using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using mvc_server.Helpers;
using mvc_server.Services;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Text.Json.Serialization;
using mvc_server.Models;
using System.Web;
using Microsoft.AspNetCore.Mvc.Formatters;
using mvc_server.Interfaces;

namespace mvc_server.Controllers;

[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private StreamedFileCompositor _fileCompositor;
    private FileMeta fileMeta;
    private readonly ILogger<StreamingController> _logger;
    public StreamingController(StreamedFileCompositor compositor, ILogger<StreamingController> logger)
    {
        _fileCompositor = compositor;
        _logger = logger;
    }


    [HttpPost("uploadlarge")]
    [DisableFormValueModelBinding]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadLargeFileAsync()
    {
        try
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                throw new FormatException("Form without multipart content.");
            }

            var subDirectory = string.Empty;
            var count = 0;
            var totalSize = 0L;
            // find the boundary
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
            // use boundary to iterator through the multipart section
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();
            int bytesRead = 0;

            do
            {


                ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
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
                    byte[] buffer = new byte[fileMeta.bytesRead];
                    await section.AsFileSection().FileStream.ReadExactlyAsync(buffer, 0, fileMeta.bytesRead);
                    RandomAccess.Write(file.Stream, buffer, fileMeta.currentPart * file.PartSize);
                    file.PartsWritten++;
                    //await filePart.FileStream.CopyToAsync(file.Stream);

                }
                //totalSize += await SaveFileAsync(section, subDirectory);

                //count++;
                section = await reader.ReadNextSectionAsync();
            } while (section != null);

            return Ok(new { Count = count, Size = Helpers.Utility.BytesToStringOptimized(totalSize) });

        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while streaming file");
            return BadRequest($"Error: {exception.Message}");
        }
    }

    [HttpPost("handshake")]
    public IActionResult Handshake(string fileName, long fileSize, int totalParts, int expectedPartSize)
    {
        if (string.IsNullOrEmpty(fileName) || fileSize < 1 || expectedPartSize <= 64)
            return BadRequest();
        try
        {
            var encodeFileName = HttpUtility.HtmlEncode(fileName);
            var UniqueID = Guid.NewGuid().ToString();


            var streamedFile = new StreamedFile
            {
                Id = UniqueID,
                FileName = fileName,
                PartSize = expectedPartSize,
                FileSize = fileSize,
                TotalFileParts = totalParts,
                Stream = System.IO.File.OpenHandle($"wwwroot/files/{encodeFileName}",
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.Write,
                            preallocationSize: fileSize),
                Created = DateTime.Now
            };
            streamedFile.CloseEvent += _fileCompositor.CloseEventHandler;

            _fileCompositor.StreamedFiles.Add(UniqueID, streamedFile);
            _logger.LogInformation($"FileHandle opened(OK) Filename: {fileName}, Filesize:{streamedFile.FileSize}");
            return Ok(UniqueID);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while handshake");
            return BadRequest($"Check the handshake data {ex.Message}");
        }




    }
    [HttpPost("abort")]
    public IActionResult AbortStreaming(string uid)
    {
        if (!_fileCompositor.StreamedFiles.TryGetValue(fileMeta.uid, out var file))
            return BadRequest("Identifier does not exist");

        file.Stream.Close();
        var fileInf = new FileInfo(Path.Combine("wwwroot", "files", file.FileName));
        fileInf.Delete();
        _fileCompositor.StreamedFiles.Remove(uid);

        return Ok(file.FileName);

    }
    private class FileMeta
    {
        public string uid { get; set; }
        public int currentPart { get; set; }
        public int bytesRead { get; set; }
    }
}