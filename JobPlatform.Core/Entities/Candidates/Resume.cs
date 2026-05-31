using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Candidates;

public sealed class Resume : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public Guid? FileId { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}