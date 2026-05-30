using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.Notifications;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public bool IsRead => ReadAt.HasValue;
}
