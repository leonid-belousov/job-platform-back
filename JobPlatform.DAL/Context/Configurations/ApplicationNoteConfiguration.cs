using JobPlatform.Core.Entities.Applications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class ApplicationNoteConfiguration : IEntityTypeConfiguration<ApplicationNote>
{
    public void Configure(EntityTypeBuilder<ApplicationNote> builder)
    {
        builder.ToTable("application_notes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => x.JobApplicationId);
        builder.HasIndex(x => x.AuthorUserId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.Notes)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.AuthorUser)
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}