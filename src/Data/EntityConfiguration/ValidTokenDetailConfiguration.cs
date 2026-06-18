using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using core.Domain;

namespace Data.EntityConfiguration;

public class ValidTokenDetailConfiguration : IEntityTypeConfiguration<ValidTokenDetail>
{
    public void Configure(EntityTypeBuilder<ValidTokenDetail> builder)
    {
        builder.HasNoKey();
        builder.ToView("v_valid_token_details");

        builder.Property(v => v.TokenId).HasColumnName("token_id");
        builder.Property(v => v.IssuerId).HasColumnName("issuer_id");
        builder.Property(v => v.IssuerName).HasColumnName("issuer_name");
        builder.Property(v => v.TokenHash).HasColumnName("token_hash");
    }
}
