namespace Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("roles")]
public class RolesEntity : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }
}
