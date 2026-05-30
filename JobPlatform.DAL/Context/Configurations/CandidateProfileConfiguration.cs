using JobPlatform.Core.Entities.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.ToTable("candidate_profiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MiddleName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Citizenship).HasMaxLength(80);
        builder.Property(x => x.CountryOfResidence).HasMaxLength(80);
        builder.Property(x => x.City).HasMaxLength(120);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.DesiredPosition).HasMaxLength(200);
        builder.Property(x => x.ExpectedSalary).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.About).HasMaxLength(4000);
        builder.Property(x => x.JobSearchStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ModerationStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ModerationComment).HasMaxLength(2000);

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.CountryOfResidence);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.JobSearchStatus);
        builder.HasIndex(x => x.ModerationStatus);
        builder.HasIndex(x => x.DesiredPosition);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Languages)
            .WithOne(x => x.CandidateProfile)
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}