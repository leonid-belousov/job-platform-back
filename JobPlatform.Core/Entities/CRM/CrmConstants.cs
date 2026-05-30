namespace JobPlatform.Core.Entities.CRM;

public static class CrmLeadTypes
{
    public const string Candidate = "candidate";
    public const string Employer = "employer";
    public const string Company = "company";
    public const string Vacancy = "vacancy";
    public const string General = "general";

    public static readonly IReadOnlyCollection<string> All = new[] { Candidate, Employer, Company, Vacancy, General };
}

public static class CrmLeadStatuses
{
    public const string New = "New";
    public const string InProgress = "InProgress";
    public const string Qualified = "Qualified";
    public const string Lost = "Lost";
    public const string Closed = "Closed";

    public static readonly IReadOnlyCollection<string> All = new[] { New, InProgress, Qualified, Lost, Closed };
}

public static class CrmTaskStatuses
{
    public const string Open = "Open";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyCollection<string> All = new[] { Open, Completed, Cancelled };
}

public static class CrmActivityTypes
{
    public const string Note = "note";
    public const string Call = "call";
    public const string Email = "email";
    public const string Meeting = "meeting";
    public const string StatusChanged = "status_changed";
    public const string TaskCreated = "task_created";
    public const string TaskCompleted = "task_completed";

    public static readonly IReadOnlyCollection<string> All = new[] { Note, Call, Email, Meeting, StatusChanged, TaskCreated, TaskCompleted };
}
