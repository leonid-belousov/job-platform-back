using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Vacancies;

namespace JobPlatform.Core.Entities.Applications;

public sealed class JobApplication : BaseEntity
{
    public Guid VacancyId { get; set; }
    public JobVacancy Vacancy { get; set; } = null!;
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Guid ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;
    public string Status { get; set; } = ApplicationStatuses.New;
    public string? CoverLetter { get; set; }
    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<InterviewInvitation> InterviewInvitations { get; set; } = [];
    public ICollection<ApplicationNote> Notes { get; set; } = [];
}