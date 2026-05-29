namespace core.Domain;

public class FileUploadStateEntity
{
    public Guid Id { get; set; }

    public string Fingerprint { get; set; } = null!;

    public uint PartsBitfield { get; set; }

    public int PartsWritten { get; set; }

    public FileWriterMeta Metadata { get; set; } = null!;
}
