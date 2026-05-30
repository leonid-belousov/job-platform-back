namespace JobPlatform.BLL.CQRS.CRM.DTO;

public sealed record CrmTimelineItemDto(
    Guid Id,
    string ItemType,
    string Type,
    string Title,
    string? Description,
    string? Status,
    Guid? UserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DueDate);
