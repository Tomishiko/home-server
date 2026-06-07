using System.ComponentModel.DataAnnotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace web.Models;

public sealed class FileHandshake
{
    //[Required]
    //[Range(64, Helpers.Utility.maxPartSize)]
    //public required int ExpectedPartSize { get; set; }

    [Required(AllowEmptyStrings = false)]
    public required string FileName { get; set; }

    [Required]
    [Range(64, long.MaxValue)]
    public required long FileSize { get; set; }

    [Required]
    [Length(32, 32)]
    public required string FileFingerprint { get; set; }

    [Required]
    public required bool IsShared { get; set; }

    //[Required]
    //public required int TotalParts { get; set; }

    //[Required]
    //public required string FileFingerprint { get; set; }
}
