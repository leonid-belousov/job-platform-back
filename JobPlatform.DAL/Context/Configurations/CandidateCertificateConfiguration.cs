using JobPlatform.Core.Entities.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public class CandidateCertificateConfiguration : IEntityTypeConfiguration<CandidateCertificate>
{
    public void Configure(EntityTypeBuilder<CandidateCertificate> builder)
    {
        builder.ToTable("candidate_certificates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Issuer).HasMaxLength(250);
        builder.Property(x => x.CredentialId).HasMaxLength(120);
        builder.Property(x => x.CredentialUrl).HasMaxLength(1000);

        builder.HasIndex(x => x.CandidateProfileId);
        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.CandidateProfile)
            .WithMany(x => x.Certificates)
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
