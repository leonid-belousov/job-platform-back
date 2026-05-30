namespace JobPlatform.BLL.Common.Models;

public sealed record AuditEvent(
    string Action,
    string? EntityType = null,
    Guid? EntityId = null,
    object? OldValue = null,
    object? NewValue = null,
    Guid? UserId = null);