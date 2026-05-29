namespace JobPlatform.BLL.CQRS.Candidates.DTO;

public sealed record CandidateProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? City,
    string? DesiredPosition,
    decimal? ExpectedSalary);