namespace JobPlatform.Core.Entities.Applications;

public static class ApplicationStatuses
{
    public const string New = "new";
    public const string UnderReview = "under_review";
    public const string Interview = "interview";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Hired = "hired";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        New,
        UnderReview,
        Interview,
        Approved,
        Rejected,
        Hired
    };

    public static bool CanSeeContacts(string status)
    {
        return string.Equals(status, Interview, StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, Hired, StringComparison.OrdinalIgnoreCase);
    }
}