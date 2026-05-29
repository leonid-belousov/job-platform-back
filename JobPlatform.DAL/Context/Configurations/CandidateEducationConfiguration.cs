using JobPlatform.Core.Entities.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public class CandidateEducationConfiguration : IEntityTypeConfiguration<CandidateEducation>
{
    public void Configure(EntityTypeBuilder<CandidateEducation> builder)
    {
        builder.ToTable("candidate_educations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InstitutionName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Faculty).HasMaxLength(200);
        builder.Property(x => x.Degree).HasMaxLength(120);
        builder.HasOne(x => x.CandidateProfile).WithMany(x => x.Educations).HasForeignKey(x => x.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
    }
}