using System.Data.Common;
using core.Domain;
using core.Models;
using core.Models.Generic;
using Microsoft.EntityFrameworkCore;

namespace core.Interfaces;

public interface IApplicationDbContext
{

    DbSet<UserEntity> Users { get; set; }
    DbSet<LogsEntity> Logs { get; set; }
    DbSet<RolesEntity> Roles { get; set; }
    DbSet<FileEntity> Files { get; set; }
    DbSet<InviteEntity> Invites { get; set; }
    DbSet<FileUploadStateEntity> FileUploadState { get; set; }
    DbSet<ValidTokenDetail> ValidTokenDetails { get; set; }

    DbConnection GetDbConnection();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    Task<UserEntity?> RemoveUserByIdStoredProcAsync(long id, string issuer);
    Task<UserEntity?> ValidateInviteTokenStoredProcAsync(byte[] hashedToken, CancellationToken ct = default);
    Task<Result<int>> ConsumeInviteTokenStoredProcAsync(byte[] tokenHash, UserCreationDto newUser);

}
