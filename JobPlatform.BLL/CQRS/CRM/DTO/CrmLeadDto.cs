namespace JobPlatform.BLL.CQRS.CRM.DTO;

public sealed record CrmLeadDto(
    Guid Id,
    string Type,
    string Name,
    string Status,
    string? Source,
    string? Description,
    Guid? ResponsibleUserId,
    Guid? CandidateProfileId,
    Guid? CompanyId,
    Guid? VacancyId,
    Guid? ApplicationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
