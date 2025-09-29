namespace Data.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("files")]
public class FileEntity : BaseEntity
{
    [Column("uuid")]
    public string UUID { get; set; } = string.Empty;

    [Column("size")]
    public ulong Size { get; set; }

    [Column("ext")]
    public string Ext { get; set; } = string.Empty;
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [ForeignKey("Owner")]
    public uint? owner_id { get; set; }

    [Column("shared")]
    public bool Public { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [NotMapped]
    public bool Private { get => !Public; set => Public = !value; }

    public UserEntity? Owner { get; set; }

}
