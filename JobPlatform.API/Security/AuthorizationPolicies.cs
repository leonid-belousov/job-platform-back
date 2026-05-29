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
        options.AddPolicy("CandidatesManageOwn", policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CandidatesManageOwn)));
    }
}
