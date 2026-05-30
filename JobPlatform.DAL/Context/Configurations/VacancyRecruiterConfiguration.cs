using JobPlatform.Core.Entities.Vacancies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class VacancyRecruiterConfiguration : IEntityTypeConfiguration<VacancyRecruiter>
{
    public void Configure(EntityTypeBuilder<VacancyRecruiter> builder)
    {
        builder.ToTable("vacancy_recruiters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.VacancyId);
        builder.HasIndex(x => x.RecruiterUserId);
        builder.HasIndex(x => new { x.VacancyId, x.RecruiterUserId }).IsUnique();
    }
}