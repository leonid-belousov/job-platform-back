using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Candidates;

public sealed class CandidateSkill : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public string SkillCode { get; set; } = string.Empty;
    public string? Name { get; set; }
}