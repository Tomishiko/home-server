using core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.EntityConfiguration;

public class FileUploadStateConfiguration : IEntityTypeConfiguration<FileUploadStateEntity>
{
    public void Configure(EntityTypeBuilder<FileUploadStateEntity> builder)
    {
        builder.ToTable("file_upload_state");

        builder.Property(x => x.Id).HasColumnName("id");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Fingerprint)
            .IsRequired()
            .HasColumnName("fingerprint")
            .HasColumnType("bytea");

        // Unique Index for the fingerprint
        builder.HasIndex(x => x.Fingerprint)
            .IsUnique()
            .HasDatabaseName("idx_file_hash");

        builder.Property(x => x.PartsBitfield)
            .HasColumnName("parts_bitfield")
            .HasConversion(v => (int)v,
                           v => (uint)v);

        builder.Property(x => x.PartsWritten)
            .HasColumnName("parts_written")
            .IsRequired();

        builder.OwnsOne(x => x.Metadata, nav =>
        {
            nav.ToJson("metadata");
        });
    }
}
