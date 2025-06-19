namespace Data.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("files")]
public class FileEntity : BaseEntity
{
    [Column("uuid")]
    public required string UUID { get; set; }

    [Column("size")]
    public ulong Size { get; set; }

    [Column("ext")]
    public required string Ext { get; set; }
    [Column("name")]
    public required string Name { get; set; }

    [ForeignKey("Owner")]
    public uint owner_id { get; set; }

    public UserEntity? Owner { get; set; }

}
