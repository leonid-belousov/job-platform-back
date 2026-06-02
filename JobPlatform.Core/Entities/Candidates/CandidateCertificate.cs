using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Candidates;

public sealed class CandidateCertificate : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Issuer { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
}
