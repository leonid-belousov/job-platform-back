using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Users;

public sealed class UserAuthToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public string? CreatedByIp { get; set; }
}
