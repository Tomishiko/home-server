namespace Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("logs")]
public class LogsEntity : BaseEntity
{
    [ForeignKey(nameof(User))]
    public uint? user_id;

    public UserEntity? User { get; set; }

    [Column("eventname")]
    public string Event { get; set; }

    [Column("time")]
    public DateTime Time { get; set; }

}

