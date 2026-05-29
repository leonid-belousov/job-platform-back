using JobPlatform.Core.Entities.Vacancies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public class JobVacancyConfiguration : IEntityTypeConfiguration<JobVacancy>
{
    public void Configure(EntityTypeBuilder<JobVacancy> builder)
    {
        builder.ToTable("job_vacancies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Requirements).HasMaxLength(8000);
        builder.Property(x => x.Responsibilities).HasMaxLength(8000);
        builder.Property(x => x.Conditions).HasMaxLength(8000);
        builder.Property(x => x.City).HasMaxLength(120);
        builder.Property(x => x.EmploymentType).HasMaxLength(100);
        builder.Property(x => x.WorkFormat).HasMaxLength(100);
        builder.Property(x => x.ExperienceLevel).HasMaxLength(100);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SalaryFrom).HasPrecision(18, 2);
        builder.Property(x => x.SalaryTo).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.Status, x.City });
        builder.HasIndex(x => x.PublishedAt);
        builder.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}