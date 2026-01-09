using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models;

[Table("invites")]
public sealed class InviteEntity : BaseEntity
{
    //[Column("token_hash")]
    public byte[] TokenHash { get; set; } = null!;

    [Column("created_by")]
    public uint CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; private set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    [Column("used_by")]
    public long? UsedBy { get; set; }
}

