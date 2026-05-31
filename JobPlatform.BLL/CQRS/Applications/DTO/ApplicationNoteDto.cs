namespace JobPlatform.BLL.CQRS.Applications.DTO;

public sealed record ApplicationNoteDto(
    Guid Id,
    Guid JobApplicationId,
    Guid CandidateProfileId,
    Guid VacancyId,
    Guid AuthorUserId,
    string AuthorName,
    string Text,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);