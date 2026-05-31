namespace JobPlatform.BLL.CQRS.Candidates.DTO;

public sealed record CandidateLanguageDto(
    Guid Id,
    string LanguageCode,
    string Level);

public sealed record CandidateExperienceDto(
    Guid Id,
    string CompanyName,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description);

public sealed record CandidateSkillDto(
    Guid Id,
    string SkillCode,
    string? Name);

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
    bool HasNoExperience,
    bool IsComplete,
    DateTimeOffset? CompletedAt,
    string ModerationStatus,
    string? ModerationComment,
    Guid? ModeratedByUserId,
    DateTimeOffset? ModeratedAt,
    IReadOnlyCollection<CandidateLanguageDto> Languages,
    IReadOnlyCollection<CandidateExperienceDto> Experiences,
    IReadOnlyCollection<CandidateSkillDto> Skills);