namespace JobPlatform.BLL.CQRS.Candidates.DTO;

public sealed record CandidateSearchItemDto(
    Guid Id,
    string FullName,
    DateOnly? DateOfBirth,
    string? Citizenship,
    string? CountryOfResidence,
    string? City,
    string? DesiredPosition,
    decimal? ExpectedSalary,
    string? Currency,
    string JobSearchStatus,
    string ModerationStatus,
    IReadOnlyCollection<CandidateLanguageDto> Languages,
    bool ContactsVisible,
    string? Email,
    string? Phone,
    DateTimeOffset CreatedAt);