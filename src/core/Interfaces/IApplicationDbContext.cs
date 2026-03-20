using core.Domain;
using Microsoft.EntityFrameworkCore;

namespace core.Interfaces;

public interface IApplicationDbContext
{

    DbSet<UserEntity> Users { get; set; }
    DbSet<LogsEntity> Logs { get; set; }
    DbSet<RolesEntity> Roles { get; set; }
    DbSet<FileEntity> Files { get; set; }
    DbSet<InviteEntity> Invites { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<UserEntity?> RemoveUserByIdStoredProc(long id, string issuer);
    Task<UserEntity?> ValidateInviteTokenStoredProc(byte[] hashedToken, CancellationToken ct = default);
}
