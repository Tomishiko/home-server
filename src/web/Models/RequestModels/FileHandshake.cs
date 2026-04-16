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
    [MaxLength(32), MinLength(32)]
    public required byte[] FileFingerprint { get; set; }//sha256 bytes

    //[Required]
    //public required int TotalParts { get; set; }

    //[Required]
    //public required string FileFingerprint { get; set; }
}
