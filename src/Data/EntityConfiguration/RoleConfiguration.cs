using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using core.Domain;

namespace Data.EntityConfiguration;

public class RolesConfiguration : BaseConfiguration<RolesEntity>
{
    public override void Configure(EntityTypeBuilder<RolesEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("roles");

        builder.Property(x => x.Name)
               .HasColumnName("name")
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
