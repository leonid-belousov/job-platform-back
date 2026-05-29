namespace JobPlatform.BLL.CQRS.Companies.DTO;

public sealed record CompanyDto(
    Guid Id,
    string Name,
    string? Description,
    string? Industry,
    string? Website,
    string Status,
    DateTimeOffset? VerifiedAt);