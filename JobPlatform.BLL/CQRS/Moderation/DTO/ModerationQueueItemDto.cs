namespace JobPlatform.BLL.CQRS.Moderation.DTO;

public sealed record ModerationQueueItemDto(
    Guid Id,
    string EntityType,
    string Title,
    string ModerationStatus,
    string? Status,
    string? Comment,
    Guid? ModeratedByUserId,
    DateTimeOffset? ModeratedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
