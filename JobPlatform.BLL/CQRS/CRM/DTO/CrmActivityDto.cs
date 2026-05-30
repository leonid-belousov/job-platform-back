namespace JobPlatform.BLL.CQRS.CRM.DTO;

public sealed record CrmActivityDto(
    Guid Id,
    Guid LeadId,
    string Type,
    string Description,
    Guid CreatedByUserId,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    DateTimeOffset CreatedAt);
