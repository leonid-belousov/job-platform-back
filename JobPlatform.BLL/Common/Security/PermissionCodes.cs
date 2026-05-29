namespace JobPlatform.BLL.Common.Security;

public static class PermissionCodes
{
    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";
    public const string CompaniesRead = "companies.read";
    public const string CompaniesManage = "companies.manage";
    public const string VacanciesRead = "vacancies.read";
    public const string VacanciesManage = "vacancies.manage";
    public const string ApplicationsRead = "applications.read";
    public const string ApplicationsManage = "applications.manage";
    public const string CandidatesManageOwn = "candidates.manage_own";
    public const string AdminFullAccess = "admin.full_access";
}
