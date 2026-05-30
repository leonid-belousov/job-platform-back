using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.Vacancies;

public sealed class VacancyRecruiter : BaseEntity
{
    public Guid VacancyId { get; set; }
    public JobVacancy Vacancy { get; set; } = null!;

    public Guid RecruiterUserId { get; set; }
    public User RecruiterUser { get; set; } = null!;

    public Guid AssignedByUserId { get; set; }
    public User AssignedByUser { get; set; } = null!;

    public string Status { get; set; } = "active";
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
}