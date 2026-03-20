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

        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.ExpiresAt)
               .HasColumnName("expires_at");

        builder.Property(x => x.UsedAt)
               .HasColumnName("used_at");

        builder.Property(x => x.UsedBy)
               .HasColumnName("used_by");

        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
