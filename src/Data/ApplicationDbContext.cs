namespace Data.Core;
using Microsoft.EntityFrameworkCore;
using core.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Log> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("users");

        modelBuilder.Entity<Log>()
            .Property(l => l.Event)
            .HasColumnName("event");
        modelBuilder.Entity<Log>()
            .Property(l => l.Time)
            .HasColumnName("time");
        modelBuilder.Entity<Log>().Property("Userid").HasColumnName("user_id");
        modelBuilder.Entity<Log>()
                    .ToTable("logs")
                    .HasNoKey()
                    .HasOne(l=>l.User);





    }

}
