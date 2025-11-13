namespace core.Models;

public record FileUploadOptions
{
    public bool UseAccelRedirect { get; init; } = false;
    public string AccelPrefix { get; init; } = "/protected/files/";
    public string StoragePath { get; init; } = "/data/files";
}

