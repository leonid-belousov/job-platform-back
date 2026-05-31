using JobPlatform.Core.Entities.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class CandidateSkillConfiguration : IEntityTypeConfiguration<CandidateSkill>
{
    public void Configure(EntityTypeBuilder<CandidateSkill> builder)
    {
        builder.ToTable("candidate_skills");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SkillCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.HasIndex(x => x.SkillCode);
        builder.HasIndex(x => new { x.CandidateProfileId, x.SkillCode }).IsUnique();
        builder.HasOne(x => x.CandidateProfile)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}