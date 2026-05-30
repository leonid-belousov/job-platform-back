namespace JobPlatform.BLL.CQRS.CRM.DTO;

public sealed record CrmTaskDto(
    Guid Id,
    Guid LeadId,
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    string Status,
    Guid? ResponsibleUserId,
    Guid CreatedByUserId,
    DateTimeOffset? CompletedAt,
    Guid? CompletedByUserId,
    DateTimeOffset CreatedAt);
