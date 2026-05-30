using JobPlatform.Core.Entities.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class CandidateLanguageConfiguration : IEntityTypeConfiguration<CandidateLanguage>
{
    public void Configure(EntityTypeBuilder<CandidateLanguage> builder)
    {
        builder.ToTable("candidate_languages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LanguageCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Level).HasMaxLength(40).IsRequired();

        builder.HasIndex(x => x.CandidateProfileId);
        builder.HasIndex(x => x.LanguageCode);
        builder.HasIndex(x => new { x.CandidateProfileId, x.LanguageCode }).IsUnique();
    }
}