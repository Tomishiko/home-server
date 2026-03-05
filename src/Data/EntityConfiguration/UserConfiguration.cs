using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using core.Domain;

namespace Data.EntityConfiguration;

public class UserConfiguration : BaseConfiguration<UserEntity>
{
    public override void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("users");

        builder.Property(x => x.Uname)
               .HasColumnName("uname")
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Password)
               .HasColumnName("password")
               .IsRequired();

        builder.Property(x => x.Email)
               .HasColumnName("email")
               .HasMaxLength(255);

        builder.Property(x => x.RoleId)
               .HasColumnName("role_id");

        // Defining the relationship
        builder.HasOne(x => x.Role)
               .WithMany()
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Uname).IsUnique();
    }
}
