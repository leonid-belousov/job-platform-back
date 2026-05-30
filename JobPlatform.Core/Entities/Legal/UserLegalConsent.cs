using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.Legal;

public sealed class UserLegalConsent : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string DocumentType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Language { get; set; } = "ru";
    public DateTimeOffset AcceptedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? IpAddress { get; set; }
}