using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Companies;

public sealed class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = CompanyTypes.DirectEmployer;
    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public Guid? LogoFileId { get; set; }
    public string Status { get; set; } = "PendingVerification";
    public string ModerationStatus { get; set; } = "Pending";
    public string? ModerationComment { get; set; }
    public Guid? ModeratedByUserId { get; set; }
    public DateTimeOffset? ModeratedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public ICollection<CompanyMember> Members { get; set; } = new List<CompanyMember>();
}