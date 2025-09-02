namespace Data.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("logs")]
public class LogsEntity : BaseEntity
{

    [Column("username")]
    public string? Uname { get; set; }

    [Column("eventname")]
    public required string Event { get; set; }

    [Column("time")]
    public required DateTime Time { get; set; }

}

