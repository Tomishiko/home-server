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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>();
        modelBuilder.Entity<LogsEntity>();
        modelBuilder.Entity<RolesEntity>();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
       optionsBuilder.LogTo(Console.WriteLine, LogLevel.Debug)
                     .EnableSensitiveDataLogging()
                     .EnableDetailedErrors();
    }

}
