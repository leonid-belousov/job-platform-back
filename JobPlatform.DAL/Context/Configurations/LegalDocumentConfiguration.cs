using JobPlatform.Core.Entities.Legal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class LegalDocumentConfiguration : IEntityTypeConfiguration<LegalDocument>
{
    public void Configure(EntityTypeBuilder<LegalDocument> builder)
    {
        builder.ToTable("legal_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.HasIndex(x => new { x.Type, x.Language, x.IsActive });
        builder.HasIndex(x => new { x.Type, x.Version, x.Language }).IsUnique();
    }
}