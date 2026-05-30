namespace JobPlatform.BLL.CQRS.Auth.DTO;

public sealed record SessionDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    bool IsActive,
    string? CreatedByIp);
