namespace JobPlatform.BLL.CQRS.Vacancies.DTO;

public sealed record VacancyRecruiterDto(
    Guid Id,
    Guid VacancyId,
    Guid RecruiterUserId,
    string RecruiterName,
    string? RecruiterEmail,
    Guid AssignedByUserId,
    string Status,
    DateTimeOffset AssignedAt);