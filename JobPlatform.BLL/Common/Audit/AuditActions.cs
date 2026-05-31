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
    public const string AuthEmailConfirmationSent = "auth.email_confirmation_sent";
    public const string AuthEmailConfirmed = "auth.email_confirmed";
    public const string AuthPasswordResetRequested = "auth.password_reset_requested";
    public const string AuthPasswordResetCompleted = "auth.password_reset_completed";

    public const string CompanyCreated = "company.created";
    public const string CompanyUpdated = "company.updated";
    public const string CompanyLogoUpdated = "company.logo_updated";
    public const string CompanyApproved = "company.approved";
    public const string CompanyRejected = "company.rejected";

    public const string VacancyCreated = "vacancy.created";
    public const string VacancyUpdated = "vacancy.updated";
    public const string VacancyPublished = "vacancy.published";
    public const string VacancyArchived = "vacancy.archived";
    public const string VacancyApproved = "vacancy.approved";
    public const string VacancyRejected = "vacancy.rejected";
    public const string VacancySubmittedForModeration = "vacancy.submitted_for_moderation";

    public const string ApplicationCreated = "application.created";
    public const string ApplicationStatusChanged = "application.status_changed";
    public const string ApplicationNoteCreated = "application.note_created";
    public const string ApplicationNoteUpdated = "application.note_updated";
    public const string ApplicationNoteDeleted = "application.note_deleted";

    public const string UserBlocked = "user.blocked";
    public const string UserUnblocked = "user.unblocked";

    public const string CandidateProfileUpdated = "candidate.profile_updated";
    public const string ResumeCreated = "resume.created";
    public const string FileUploaded = "file.uploaded";
    
    public const string DictionaryItemCreated = "dictionary.item_created";
    public const string DictionaryItemUpdated = "dictionary.item_updated";
    public const string DictionaryItemActivated = "dictionary.item_activated";
    public const string DictionaryItemDeactivated = "dictionary.item_deactivated";
    
    public const string CrmLeadCreated = "crm.lead_created";
    public const string CrmLeadUpdated = "crm.lead_updated";
    public const string CrmTaskCreated = "crm.task_created";
    public const string CrmTaskCompleted = "crm.task_completed";
    public const string CrmActivityAdded = "crm.activity_added";
    
    public const string QuestionnaireCreated = "questionnaire.created";
    public const string QuestionnaireUpdated = "questionnaire.updated";
    public const string QuestionnaireSectionAdded = "questionnaire.section_added";
    public const string QuestionnaireQuestionAdded = "questionnaire.question_added";
    public const string QuestionnaireOptionAdded = "questionnaire.option_added";
    public const string QuestionnaireResponseSubmitted = "questionnaire.response_submitted";
}