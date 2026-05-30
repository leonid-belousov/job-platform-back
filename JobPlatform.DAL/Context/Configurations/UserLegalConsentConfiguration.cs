using JobPlatform.Core.Entities.Legal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class UserLegalConsentConfiguration : IEntityTypeConfiguration<UserLegalConsent>
{
    public void Configure(EntityTypeBuilder<UserLegalConsent> builder)
    {
        builder.ToTable("user_legal_consents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(100);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.DocumentType, x.Version, x.Language }).IsUnique();
    }
}