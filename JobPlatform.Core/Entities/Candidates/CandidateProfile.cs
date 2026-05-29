using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.Candidates;

public sealed class CandidateProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? DesiredPosition { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? About { get; set; }
    public bool IsVisible { get; set; } = true;
    public string JobSearchStatus { get; set; } = "ActiveSearch";
    public ICollection<CandidateExperience> Experiences { get; set; } = [];
    public ICollection<CandidateEducation> Educations { get; set; } = [];
    public ICollection<Resume> Resumes { get; set; } = [];
}