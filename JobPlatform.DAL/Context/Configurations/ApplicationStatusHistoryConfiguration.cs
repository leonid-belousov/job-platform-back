using JobPlatform.Core.Entities.Applications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
    {
        builder.ToTable("application_status_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OldStatus).HasMaxLength(50);
        builder.Property(x => x.NewStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasIndex(x => x.JobApplicationId);
        builder.HasOne(x => x.JobApplication).WithMany(x => x.StatusHistory).HasForeignKey(x => x.JobApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}