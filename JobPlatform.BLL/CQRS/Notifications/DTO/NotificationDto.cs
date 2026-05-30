namespace JobPlatform.BLL.CQRS.Notifications.DTO;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? EntityType,
    Guid? EntityId,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);
