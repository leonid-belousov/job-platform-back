using JobPlatform.Core.Entities.Applications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class InterviewInvitationConfiguration : IEntityTypeConfiguration<InterviewInvitation>
{
    public void Configure(EntityTypeBuilder<InterviewInvitation> builder)
    {
        builder.ToTable("interview_invitations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Format).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.MeetingUrl).HasMaxLength(1000);
        builder.Property(x => x.Message).HasMaxLength(2000);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ScheduledAt);
    }
}