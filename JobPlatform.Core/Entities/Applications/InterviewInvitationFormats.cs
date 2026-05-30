namespace JobPlatform.Core.Entities.Applications;

public static class InterviewInvitationFormats
{
    public const string Online = "online";
    public const string Offline = "offline";
    public const string Phone = "phone";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Online,
        Offline,
        Phone
    };
}