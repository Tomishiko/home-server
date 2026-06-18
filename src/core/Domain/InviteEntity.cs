using System.Diagnostics.Contracts;
using core.Models;

namespace core.Domain;

public sealed class InviteEntity : BaseEntity
{
    public byte[] TokenHash { get; set; } = null!;

    public long CreatedById { get; set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public long? UsedById { get; set; }

    // Nav properties
    public UserEntity? UsedBy { get; set; }
    public UserEntity? CreatedBy { get; set; }
}

