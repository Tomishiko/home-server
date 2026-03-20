using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace web.Models;

public sealed class FilePartMetaData
{
    [Required]
    [JsonPropertyName("uid")]
    public Guid Uid { get; set; }

    [Required]
    [JsonPropertyName("currentPart")]
    public int CurrentPart { get; set; }

    [Required]
    [JsonPropertyName("bytesRead")]
    public int BytesRead { get; set; }
}

