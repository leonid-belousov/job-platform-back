namespace JobPlatform.BLL.CQRS.Vacancies.DTO;

public sealed record VacancyDto(
    Guid Id,
    string Title,
    string? City,
    string? EmploymentType,
    string? WorkFormat,
    string? ExperienceLevel,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string? Currency,
    string Status,
    string? ModerationStatus = null,
    string? ModerationComment = null,
    Guid? ModeratedByUserId = null,
    DateTimeOffset? ModeratedAt = null);