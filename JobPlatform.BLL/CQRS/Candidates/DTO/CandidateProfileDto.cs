namespace JobPlatform.BLL.CQRS.Candidates.DTO;

public sealed record CandidateLanguageDto(
    Guid Id,
    string LanguageCode,
    string Level);

public sealed record CandidateProfileDto(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly? DateOfBirth,
    string? Citizenship,
    string? CountryOfResidence,
    string? City,
    string? Phone,
    string? DesiredPosition,
    decimal? ExpectedSalary,
    string? Currency,
    string? About,
    bool IsVisible,
    string JobSearchStatus,
    string ModerationStatus,
    string? ModerationComment,
    Guid? ModeratedByUserId,
    DateTimeOffset? ModeratedAt,
    IReadOnlyCollection<CandidateLanguageDto> Languages);