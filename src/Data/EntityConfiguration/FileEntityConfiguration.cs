using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using core.Domain;

namespace Data.EntityConfiguration;

public class FileConfiguration : BaseConfiguration<FileEntity>
{
    public override void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("files");

        builder.Property(x => x.UUID)
               .HasColumnName("uuid")
               .IsRequired();

        builder.Property(x => x.Size)
               .HasColumnName("size");

        builder.Property(x => x.Ext)
               .HasColumnName("ext");

        builder.Property(x => x.Name)
               .HasColumnName("name");

        builder.Property(x => x.IsPublic)
               .HasColumnName("shared");

        builder.Property(x => x.IsDeleted)
               .HasColumnName("is_deleted");

        builder.Ignore(x => x.IsPrivate);

        builder.Property(x => x.OwnerId)
               .HasColumnName("owner_id");

        builder.HasOne(x => x.Owner)
               .WithMany()
               .HasForeignKey(x => x.OwnerId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
