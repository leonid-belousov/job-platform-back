using JobPlatform.Core.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class ContentPageConfiguration : IEntityTypeConfiguration<ContentPage>
{
    public void Configure(EntityTypeBuilder<ContentPage> builder)
    {
        builder.ToTable("content_pages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(120).IsRequired();
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.JsonContent).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.IsPublished).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
