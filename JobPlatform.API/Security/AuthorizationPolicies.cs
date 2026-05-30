using JobPlatform.BLL.Common.Security;
using Microsoft.AspNetCore.Authorization;

namespace JobPlatform.API.Security;

public static class AuthorizationPolicies
{
    public static void AddRecruitmentPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
        options.AddPolicy("UsersRead", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.UsersRead)));
        options.AddPolicy("UsersManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.UsersManage)));
        options.AddPolicy("CompaniesManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CompaniesManage)));
        options.AddPolicy("VacanciesManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.VacanciesManage)));
        options.AddPolicy("ApplicationsManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.ApplicationsManage)));
        options.AddPolicy("CandidatesRead", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CandidatesRead)));
        options.AddPolicy("CandidatesManageOwn", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CandidatesManageOwn)));
        options.AddPolicy("DictionariesRead", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.DictionariesRead)));
        options.AddPolicy("DictionariesManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.DictionariesManage)));
        options.AddPolicy("ModerationRead", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.ModerationRead)));
        options.AddPolicy("ModerationManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.ModerationManage)));
        options.AddPolicy("CrmRead", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CrmRead)));
        options.AddPolicy("CrmManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CrmManage)));
        options.AddPolicy("QuestionnairesRead", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.QuestionnairesRead)));
        options.AddPolicy("QuestionnairesManage", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.QuestionnairesManage)));
        options.AddPolicy("AnalyticsRead", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.AnalyticsRead)));
    }
}