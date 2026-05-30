namespace JobPlatform.BLL.CQRS.Audit.DTO;

public sealed record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string Action,
    string? EntityType,
    Guid? EntityId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt);