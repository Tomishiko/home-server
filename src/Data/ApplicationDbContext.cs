namespace Data.Core;
using Microsoft.EntityFrameworkCore;
using Data.Shared;
using Data.Models;
using Microsoft.Extensions.Logging;

public class ApplicationDbContext : DbContext
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
        modelBuilder.Entity<UserEntity>();
        modelBuilder.Entity<LogsEntity>();
        modelBuilder.Entity<RolesEntity>();
        modelBuilder.Entity<FileEntity>();
        modelBuilder.Entity<InviteEntity>(e =>
                {
                    e.Property(i => i.TokenHash)
                     .HasColumnName("token_hash")
                     .HasColumnType("bytea")
                     .IsRequired();
                    e.Property(i => i.CreatedAt)
                     .HasColumnType("timestamptz")
                     .HasDefaultValueSql("now()")
                     .ValueGeneratedOnAdd();
                });
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.LogTo(Console.WriteLine, LogLevel.Debug)
        //              .EnableSensitiveDataLogging()
        //              .EnableDetailedErrors();
    }

}
