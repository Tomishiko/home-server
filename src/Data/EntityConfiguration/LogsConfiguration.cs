using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using core.Domain;

namespace Data.EntityConfiguration;

public class LogsConfiguration : BaseConfiguration<LogsEntity>
{
    public override void Configure(EntityTypeBuilder<LogsEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("logs");

        builder.Property(x => x.Uname)
               .HasColumnName("username")
               .HasMaxLength(50);

        builder.Property(x => x.Event)
               .HasColumnName("event_name")
               .IsRequired();

        builder.Property(x => x.Time)
               .HasColumnName("created_at")
               .IsRequired();

        builder.HasIndex(x => x.Time);
    }
}
