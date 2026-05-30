namespace JobPlatform.BLL.Common.Interfaces;

public interface INotificationService
{
    Task CreateInternalAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default);

    Task CreateInternalForUsersAsync(
        IEnumerable<Guid> userIds,
        string type,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default);
}
