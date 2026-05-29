namespace JobPlatform.BLL.CQRS.Applications.DTO;

public sealed record ApplicationDto(
    Guid Id,
    Guid VacancyId,
    string VacancyTitle,
    Guid CandidateProfileId,
    string CandidateName,
    Guid ResumeId,
    string Status,
    string? CoverLetter,
    DateTimeOffset CreatedAt);