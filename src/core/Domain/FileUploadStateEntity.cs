namespace core.Domain;

public class FileUploadStateEntity
{
    public Guid Id { get; set; }

    public byte[] Fingerprint { get; set; } = null!;

    public byte[]? PartsBitfield { get; set; }

    public int PartsWritten { get; set; }

    public FileWriterMeta Metadata { get; set; } = null!;
}
