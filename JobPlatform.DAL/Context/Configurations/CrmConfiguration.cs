using JobPlatform.Core.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class CrmLeadConfiguration : IEntityTypeConfiguration<CrmLead>
{
    public void Configure(EntityTypeBuilder<CrmLead> builder)
    {
        builder.ToTable("crm_leads");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ResponsibleUserId);
        builder.HasIndex(x => x.CandidateProfileId);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.VacancyId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasOne(x => x.ResponsibleUser).WithMany().HasForeignKey(x => x.ResponsibleUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CrmTaskConfiguration : IEntityTypeConfiguration<CrmTask>
{
    public void Configure(EntityTypeBuilder<CrmTask> builder)
    {
        builder.ToTable("crm_tasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.LeadId);
        builder.HasIndex(x => x.ResponsibleUserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DueDate);
        builder.HasOne(x => x.Lead).WithMany(x => x.Tasks).HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ResponsibleUser).WithMany().HasForeignKey(x => x.ResponsibleUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CrmActivityConfiguration : IEntityTypeConfiguration<CrmActivity>
{
    public void Configure(EntityTypeBuilder<CrmActivity> builder)
    {
        builder.ToTable("crm_activities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.RelatedEntityType).HasMaxLength(100);
        builder.HasIndex(x => x.LeadId);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasOne(x => x.Lead).WithMany(x => x.Activities).HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Cascade);
    }
}