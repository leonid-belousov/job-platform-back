using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Candidates;

public sealed class CandidateEducation : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public string InstitutionName { get; set; } = string.Empty;
    public string? Faculty { get; set; }
    public string? Degree { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
}