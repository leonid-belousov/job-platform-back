using JobPlatform.API.Security;
using JobPlatform.BLL.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace JobPlatform.API.Tests.Auth;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void AddRecruitmentPolicies_RegistersPoliciesForSeededPermissionCodes()
    {
        var options = new AuthorizationOptions();

        options.AddRecruitmentPolicies();

        AssertPermissionPolicy(options, "CompaniesRead", PermissionCodes.CompaniesRead);
        AssertPermissionPolicy(options, "CompaniesManage", PermissionCodes.CompaniesManage);
        AssertPermissionPolicy(options, "VacanciesRead", PermissionCodes.VacanciesRead);
        AssertPermissionPolicy(options, "VacanciesManage", PermissionCodes.VacanciesManage);
        AssertPermissionPolicy(options, "ApplicationsRead", PermissionCodes.ApplicationsRead);
        AssertPermissionPolicy(options, "ApplicationsManage", PermissionCodes.ApplicationsManage);
        AssertPermissionPolicy(options, "CandidatesRead", PermissionCodes.CandidatesRead);
        AssertPermissionPolicy(options, "CandidatesManageOwn", PermissionCodes.CandidatesManageOwn);
        AssertPermissionPolicy(options, "ModerationRead", PermissionCodes.ModerationRead);
        AssertPermissionPolicy(options, "ModerationManage", PermissionCodes.ModerationManage);
    }

    [Fact]
    public void AddRecruitmentPolicies_RegistersAdminOnlyAsRolePolicy()
    {
        var options = new AuthorizationOptions();

        options.AddRecruitmentPolicies();

        var policy = options.GetPolicy("AdminOnly");

        Assert.NotNull(policy);
        var rolesRequirement = Assert.Single(policy!.Requirements.OfType<RolesAuthorizationRequirement>());
        Assert.Contains("admin", rolesRequirement.AllowedRoles);
    }

    private static void AssertPermissionPolicy(AuthorizationOptions options, string policyName, string permissionCode)
    {
        var policy = options.GetPolicy(policyName);

        Assert.NotNull(policy);
        var requirement = Assert.Single(policy!.Requirements.OfType<PermissionRequirement>());
        Assert.Equal(permissionCode, requirement.Permission);
    }
}
