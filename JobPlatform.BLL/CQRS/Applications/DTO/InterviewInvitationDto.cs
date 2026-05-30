namespace JobPlatform.BLL.CQRS.Applications.DTO;

public sealed record InterviewInvitationDto(
    Guid Id,
    Guid ApplicationId,
    DateTimeOffset ScheduledAt,
    string Format,
    string? Location,
    string? MeetingUrl,
    string? Message,
    string Status,
    Guid CreatedByUserId,
    DateTimeOffset? CandidateRespondedAt,
    DateTimeOffset CreatedAt);