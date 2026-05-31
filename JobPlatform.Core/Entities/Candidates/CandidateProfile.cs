using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.Candidates;

public sealed class CandidateProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }
    public string? Citizenship { get; set; }
    public string? CountryOfResidence { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }

    public string? DesiredPosition { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? Currency { get; set; }
    public string? About { get; set; }

    public bool IsVisible { get; set; } = true;
    public string JobSearchStatus { get; set; } = CandidateJobSearchStatuses.ActiveSearch;
    public bool HasNoExperience { get; set; }
    public bool IsComplete { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public string ModerationStatus { get; set; } = "Pending";
    public string? ModerationComment { get; set; }
    public Guid? ModeratedByUserId { get; set; }
    public DateTimeOffset? ModeratedAt { get; set; }

    public ICollection<CandidateExperience> Experiences { get; set; } = [];
    public ICollection<CandidateEducation> Educations { get; set; } = [];
    public ICollection<CandidateLanguage> Languages { get; set; } = [];
    public ICollection<CandidateSkill> Skills { get; set; } = [];
    public ICollection<Resume> Resumes { get; set; } = [];
}