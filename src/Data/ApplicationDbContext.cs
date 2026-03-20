using Microsoft.EntityFrameworkCore;
using core.Domain;
using core.Interfaces;
using core.Models.Generic;
using Npgsql;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Data.Core;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<LogsEntity> Logs { get; set; }
    public DbSet<RolesEntity> Roles { get; set; }
    public DbSet<FileEntity> Files { get; set; }
    public DbSet<InviteEntity> Invites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.LogTo(Console.WriteLine, LogLevel.Debug)
        //              .EnableSensitiveDataLogging()
        //              .EnableDetailedErrors();
    }

    public async Task<UserEntity?> RemoveUserByIdStoredProc(long id, string issuer)
    {
        // FK link is removed by pstgres function
        var userIdParam = new NpgsqlParameter
        {
            ParameterName = "p_user_id",
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bigint,
            Value = id
        };
        var issuerNameParam = new NpgsqlParameter
        {
            ParameterName = "p_issuer_name",
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar,
            Value = issuer
        };

        UserEntity? deleted = await Users
            .FromSqlRaw("SELECT * FROM remove_user_by_id(@p_user_id,@p_issuer_name)",
                        userIdParam,
                        issuerNameParam)
            .SingleOrDefaultAsync();

        return deleted;

    }
    public async Task<UserEntity?> ValidateInviteTokenStoredProc(byte[] hashedToken, CancellationToken ct = default)
    {
        var tokenParam = new NpgsqlParameter
        {
            ParameterName = "token",
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bytea,
            Value = hashedToken
        };

        UserEntity? issuer = await Users
                            .FromSqlRaw($"SELECT * FROM GetInviteIssuerByToken(@token)",
                                         tokenParam)
                            .AsNoTracking()
                            .SingleOrDefaultAsync(ct);
        return issuer;


    }
}
