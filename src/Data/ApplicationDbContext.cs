using Microsoft.EntityFrameworkCore;
using core.Domain;
using core.Interfaces;
using Npgsql;
using NpgsqlTypes;
using System.Data.Common;

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
    public DbSet<FileUploadStateEntity> FileUploadState { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    public DbConnection GetDbConnection() => base.Database.GetDbConnection();

    public async Task<UserEntity?> RemoveUserByIdStoredProcAsync(long id,
                                                                 string issuer)
    {
        // FK link is removed by pstgres function
        var userIdParam = new NpgsqlParameter
        {
            ParameterName = "p_user_id",
            NpgsqlDbType = NpgsqlDbType.Bigint,
            Value = id
        };
        var issuerNameParam = new NpgsqlParameter
        {
            ParameterName = "p_issuer_name",
            NpgsqlDbType = NpgsqlDbType.Varchar,
            Value = issuer
        };

        UserEntity? deleted = await Users
            .FromSqlRaw("SELECT * FROM remove_user_by_id(@p_user_id,@p_issuer_name)",
                        userIdParam,
                        issuerNameParam)
            .AsNoTracking()
            .SingleOrDefaultAsync();

        return deleted;

    }
    public async Task<UserEntity?> ValidateInviteTokenStoredProcAsync(byte[] hashedToken, CancellationToken ct = default)
    {
        var tokenParam = new NpgsqlParameter
        {
            ParameterName = "token",
            NpgsqlDbType = NpgsqlDbType.Bytea,
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
