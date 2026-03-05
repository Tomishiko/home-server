using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public sealed class MetaData
{
    [Required]
    public Guid Uid { get; set; }

    [Required]
    public int CurrentPart { get; set; }

    [Required]
    public int BytesRead { get; set; }
}

public sealed class FilePartUploadRequest
{
    [FromForm(Name = "meta")]
    [Required]
    public required string MetaJson { get; set; }

    [FromForm(Name = "file")]
    [Required]
    public required IFormFile File { get; set; }

    public MetaData? GetMetaData() =>
        JsonSerializer.Deserialize<MetaData>(MetaJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
}
