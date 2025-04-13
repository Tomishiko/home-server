namespace Data.Core;
using Microsoft.EntityFrameworkCore;
using Data.Shared;
using Data.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<LogsEntity> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.UseSnakeCaseNames();
        modelBuilder.Entity<UserEntity>();
        modelBuilder.Entity<LogsEntity>();
    }

}
