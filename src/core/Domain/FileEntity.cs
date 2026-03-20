namespace core.Domain;

public class FileEntity : BaseEntity
{
    public string UUID { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Ext { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public long? OwnerId { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDeleted { get; set; } = false;

    public bool IsPrivate
    {
        get => !IsPublic;
        set => IsPublic = !value;
    }

    public UserEntity? Owner { get; set; }
}
