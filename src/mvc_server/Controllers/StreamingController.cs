using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using mvc_server.Helpers;
using mvc_server.Service;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using mvc_server.Models;


namespace mvc_server.Controllers;
//TODO: here is a good article on chunks upload
//https://www.c-sharpcorner.com/article/upload-large-files-to-mvc-webapi-using-partitioning/
[Route("api/[controller]")]
public class StremingController : ControllerBase
{
    private StreamedFileCompositor _fileCompositor;
    private string _uniqueID;
    private int _currentPart;
    public StremingController(StreamedFileCompositor compositor)
    {
        _fileCompositor = compositor;
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
            do
            {
                //TODO: Optimize this routing


                ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition) && contentDisposition.Name == "meta")
                    {
                        var formData = await section.AsFormDataSection().GetValueAsync();
                        (_uniqueID, _currentPart) = JsonSerializer.Deserialize<(string, int)>(
                            formData,
                            new JsonSerializerOptions { IncludeFields = true });

                    }
                    section = await reader.ReadNextSectionAsync();
                    continue;
                }

                StreamedFile file;

                if (_fileCompositor.StreamedFiles.TryGetValue(_uniqueID, out file))
                {
                    var filePart = section.AsFileSection();
                    await filePart.FileStream.CopyToAsync(file.Stream);

                }
                if (_currentPart == file?.TotalFileParts)
                {
                    file.Stream.Close();
                    _fileCompositor.StreamedFiles.Remove(_uniqueID);
                }


                //totalSize += await SaveFileAsync(section, subDirectory);

                //count++;
                section = await reader.ReadNextSectionAsync();
            } while (section != null);

            return Ok(new { Count = count, Size = Helpers.Utility.BytesToStringOptimized(totalSize) });

        }
        catch (Exception exception)
        {
            return BadRequest($"Error: {exception.Message}");
        }
    }

    [HttpPost]
    public IActionResult HandShake(string fileName, int totalParts, int partSize)
    {
        if (string.IsNullOrEmpty(fileName) || totalParts < 1 || partSize <= 64)
            return BadRequest();

        var streamedFile = new StreamedFile
        {
            FileName = fileName,
            PartSize = partSize,
            TotalFileParts = totalParts,
            Stream = new FileStream($"wwwroot/files/{fileName}", FileMode.Append, FileAccess.Write, FileShare.Write)
        };

        var UniqueID = Guid.NewGuid().ToString();
        _fileCompositor.StreamedFiles.Add(UniqueID, streamedFile);

        return Ok(UniqueID);
    }

}