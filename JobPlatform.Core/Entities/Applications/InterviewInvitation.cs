using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Applications;

public sealed class InterviewInvitation : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;

    public DateTimeOffset ScheduledAt { get; set; }
    public string Format { get; set; } = InterviewInvitationFormats.Online;
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = InterviewInvitationStatuses.Pending;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset? CandidateRespondedAt { get; set; }
}