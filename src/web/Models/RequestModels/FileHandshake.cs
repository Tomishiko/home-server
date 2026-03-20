using System.ComponentModel.DataAnnotations;

namespace web.Models;

public sealed class FileHandshake
{
    [Required]
    [Range(64, Helpers.Utility.maxPartSize)]
    public uint ExpectedPartSize { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [Range(64, long.MaxValue)]
    public long FileSize { get; set; }

    [Required]
    public uint TotalParts { get; set; }
}
