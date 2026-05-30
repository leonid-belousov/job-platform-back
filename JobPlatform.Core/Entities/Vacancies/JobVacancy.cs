using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Companies;

namespace JobPlatform.Core.Entities.Vacancies;

public sealed class JobVacancy : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Conditions { get; set; }
    public string? City { get; set; }
    public string? EmploymentType { get; set; }
    public string? WorkFormat { get; set; }
    public string? ExperienceLevel { get; set; }
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public string? Currency { get; set; }
    public string Status { get; set; } = "Draft";
    public string ModerationStatus { get; set; } = "Pending";
    public string? ModerationComment { get; set; }
    public Guid? ModeratedByUserId { get; set; }
    public DateTimeOffset? ModeratedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public ICollection<VacancyRecruiter> Recruiters { get; set; } = [];
}