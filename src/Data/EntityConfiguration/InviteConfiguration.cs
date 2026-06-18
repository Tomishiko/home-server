using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using core.Domain;

namespace Data.EntityConfiguration;

public class InviteConfiguration : BaseConfiguration<InviteEntity>
{
    public override void Configure(EntityTypeBuilder<InviteEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("invites");

        builder.Property(x => x.TokenHash)
               .HasColumnName("token_hash")
               .HasColumnType("bytea")
               .IsRequired();

        builder.Property(x => x.CreatedById)
               .HasColumnName("created_by");

        builder.HasOne(x => x.CreatedBy)
               .WithMany()
               .HasForeignKey(x => x.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.ExpiresAt)
               .HasColumnName("expires_at")
               .HasColumnType("timestamptz");

        builder.Property(x => x.UsedAt)
               .HasColumnName("used_at")
               .HasColumnType("timestamptz");

        builder.Property(x => x.UsedById)
               .HasColumnName("used_by");

        builder.HasOne(x => x.UsedBy)
               .WithMany()
               .HasForeignKey(x => x.UsedById)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
