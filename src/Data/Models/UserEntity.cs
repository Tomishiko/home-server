namespace Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class UserEntity : BaseEntity
{
    [Column("uname")]
    public string Uname { get; set; }

    [Column("password")]
    public string Password { get; set; }

    [ForeignKey(nameof(Role))]
    public uint? role_id { get; set; }

    public RolesEntity? Role { get; set; }
}
