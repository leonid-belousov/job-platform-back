namespace JobPlatform.BLL.CQRS.Analytics.DTO;

public sealed record StatusCountDto(string Status, int Count);

public sealed record AdminDashboardDto(
    int TotalUsers,
    int ActiveUsers,
    int BlockedUsers,
    int CandidateProfiles,
    int CompaniesTotal,
    int CompaniesPendingModeration,
    int CompaniesApproved,
    int CompaniesRejected,
    int VacanciesTotal,
    int VacanciesPublished,
    int VacanciesPendingModeration,
    int VacanciesArchived,
    int VacanciesRejected,
    int ApplicationsTotal,
    IReadOnlyCollection<StatusCountDto> ApplicationsByStatus,
    int CrmLeadsTotal,
    int CrmOpenTasks,
    int CrmOverdueTasks,
    int QuestionnaireResponsesTotal,
    DateTimeOffset GeneratedAt);

public sealed record VacancyAnalyticsDto(
    Guid VacancyId,
    string Title,
    string Status,
    string ModerationStatus,
    int ApplicationsTotal,
    IReadOnlyCollection<StatusCountDto> ApplicationsByStatus);

public sealed record EmployerDashboardDto(
    Guid CompanyId,
    string CompanyName,
    int VacanciesTotal,
    int VacanciesPublished,
    int VacanciesPendingModeration,
    int ApplicationsTotal,
    int ApplicationsNew,
    int ApplicationsInProgress,
    int ApplicationsInterview,
    IReadOnlyCollection<StatusCountDto> ApplicationsByStatus,
    IReadOnlyCollection<VacancyAnalyticsDto> Vacancies);

public sealed record RecruiterDashboardDto(
    int AssignedCrmLeadsTotal,
    int AssignedCrmLeadsInProgress,
    int AssignedOpenTasks,
    int AssignedOverdueTasks,
    int ManagedVacanciesTotal,
    int ManagedVacanciesPublished,
    int ManagedApplicationsTotal,
    IReadOnlyCollection<StatusCountDto> CrmLeadsByStatus,
    IReadOnlyCollection<StatusCountDto> TasksByStatus,
    IReadOnlyCollection<StatusCountDto> ApplicationsByStatus,
    DateTimeOffset GeneratedAt);
