namespace JobPlatform.BLL.CQRS.Applications.DTO;

public sealed record ApplicationDto(
    Guid Id,
    Guid VacancyId,
    string VacancyTitle,
    Guid CandidateProfileId,
    string CandidateName,
    string? CandidateEmail,
    string? CandidatePhone,
    bool CandidateContactsVisible,
    Guid ResumeId,
    string Status,
    string? CoverLetter,
    DateTimeOffset CreatedAt);