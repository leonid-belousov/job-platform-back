using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.Core.Entities.Notifications;
using JobPlatform.DAL.Interfaces;

namespace JobPlatform.BLL.Common.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateInternalAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        await _db.Set<Notification>().AddAsync(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            EntityType = entityType,
            EntityId = entityId
        });
    }

    public async Task CreateInternalForUsersAsync(
        IEnumerable<Guid> userIds,
        string type,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var distinctUserIds = userIds.Where(x => x != Guid.Empty).Distinct().ToArray();
        foreach (var userId in distinctUserIds)
        {
            await CreateInternalAsync(userId, type, title, message, entityType, entityId, cancellationToken);
        }
    }
}
