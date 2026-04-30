using System.ComponentModel.DataAnnotations;

namespace web.Models;

public class FileUploadOptionsClient
{
    public const string SectionName = "FileUpload";

    //[Range(5242880, int.MaxValue, ErrorMessage = "Part size must be at least 5MB.")]
    public int PartSize { get; set; }
}
