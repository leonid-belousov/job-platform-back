namespace JobPlatform.BLL.Common.Audit;

public static class AuditActions
{
    public const string AuthRegistered = "auth.registered";
    public const string AuthLoginSucceeded = "auth.login_succeeded";
    public const string AuthLogout = "auth.logout";
    public const string AuthTokenRefreshed = "auth.token_refreshed";
    public const string AuthSessionRevoked = "auth.session_revoked";
    public const string AuthAllSessionsRevoked = "auth.all_sessions_revoked";
    public const string AuthUserSessionsRevokedByAdmin = "auth.user_sessions_revoked_by_admin";

    public const string CompanyCreated = "company.created";
    public const string CompanyUpdated = "company.updated";
    public const string CompanyLogoUpdated = "company.logo_updated";
    public const string CompanyApproved = "company.approved";
    public const string CompanyRejected = "company.rejected";

    public const string VacancyCreated = "vacancy.created";
    public const string VacancyPublished = "vacancy.published";
    public const string VacancyArchived = "vacancy.archived";
    public const string VacancyApproved = "vacancy.approved";
    public const string VacancyRejected = "vacancy.rejected";
    public const string VacancySubmittedForModeration = "vacancy.submitted_for_moderation";

    public const string ApplicationCreated = "application.created";
    public const string ApplicationStatusChanged = "application.status_changed";

    public const string UserBlocked = "user.blocked";
    public const string UserUnblocked = "user.unblocked";

    public const string CandidateProfileUpdated = "candidate.profile_updated";
    public const string ResumeCreated = "resume.created";
    public const string FileUploaded = "file.uploaded";
    
    public const string DictionaryItemCreated = "dictionary.item_created";
    public const string DictionaryItemUpdated = "dictionary.item_updated";
    public const string DictionaryItemActivated = "dictionary.item_activated";
    public const string DictionaryItemDeactivated = "dictionary.item_deactivated";
}