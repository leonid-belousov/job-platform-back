using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Candidates;

public sealed class CandidateLanguage : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string LanguageCode { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}