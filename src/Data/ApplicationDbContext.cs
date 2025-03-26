namespace Data.Core;
using Microsoft.EntityFrameworkCore;
using core.Models;
using Data.Shared;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Log> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSnakeCaseNames();
        modelBuilder.Entity<User>();

        modelBuilder.Entity<Log>()
                    .HasNoKey()
                    .Navigation(l=>l.User)
                    .UsePropertyAccessMode(PropertyAccessMode.Property);

    }

}
