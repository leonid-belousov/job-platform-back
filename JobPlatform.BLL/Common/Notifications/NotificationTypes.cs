namespace JobPlatform.BLL.Common.Notifications;

public static class NotificationTypes
{
    public const string ApplicationCreated = "application.created";
    public const string ApplicationStatusChanged = "application.status_changed";
    public const string InterviewInvitationCreated = "interview.invitation_created";
    public const string InterviewInvitationResponded = "interview.invitation_responded";
    public const string ReminderNewApplications = "reminder.new_applications";
    public const string ReminderInactiveVacancy = "reminder.inactive_vacancy";
}
