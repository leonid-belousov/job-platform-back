namespace JobPlatform.Core.Entities.Applications;

public static class InterviewInvitationStatuses
{
    public const string Pending = "pending";
    public const string Accepted = "accepted";
    public const string Declined = "declined";
    public const string Cancelled = "cancelled";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Pending,
        Accepted,
        Declined,
        Cancelled
    };
}