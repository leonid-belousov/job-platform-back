namespace JobPlatform.BLL.CQRS.Companies.DTO;

public sealed record CompanyDto(
    Guid Id,
    string Name,
    string Type,
    string? Description,
    string? Industry,
    string? Website,
    Guid? LogoFileId,
    string Status,
    DateTimeOffset? VerifiedAt,
    string? ModerationStatus = null,
    string? ModerationComment = null,
    Guid? ModeratedByUserId = null,
    DateTimeOffset? ModeratedAt = null);