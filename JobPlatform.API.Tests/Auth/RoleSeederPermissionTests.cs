using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Context;
using JobPlatform.DAL.Seeds;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.API.Tests.Auth;

public sealed class RoleSeederPermissionTests
{
    [Fact]
    public async Task SeedAsync_CreatesExpectedSystemRoles()
    {
        await using var db = CreateDbContext();
        var seeder = new RoleSeeder(db);

        await seeder.SeedAsync();

        var roles = await db.Set<Role>().OrderBy(x => x.Code).ToArrayAsync();

        Assert.Collection(roles,
            role => AssertRole(role, "admin", true, true),
            role => AssertRole(role, "candidate", true, false),
            role => AssertRole(role, "employer", true, false),
            role => AssertRole(role, "recruiter", true, false));
    }

    [Fact]
    public async Task SeedAsync_AssignsCandidatePermissions()
    {
        await using var db = CreateDbContext();
        await new RoleSeeder(db).SeedAsync();

        var permissions = await GetPermissionCodesAsync(db, "candidate");

        Assert.Contains("candidates.manage_own", permissions);
        Assert.Contains("vacancies.read", permissions);
        Assert.Contains("applications.read", permissions);
        Assert.DoesNotContain("vacancies.manage", permissions);
        Assert.DoesNotContain("applications.manage", permissions);
        Assert.DoesNotContain("candidates.read", permissions);
        Assert.DoesNotContain("moderation.manage", permissions);
        Assert.DoesNotContain("admin.full_access", permissions);
    }

    [Fact]
    public async Task SeedAsync_AssignsEmployerPermissions_ForVacanciesApplicationsAndAnalytics()
    {
        await using var db = CreateDbContext();
        await new RoleSeeder(db).SeedAsync();

        var permissions = await GetPermissionCodesAsync(db, "employer");

        Assert.Contains("companies.manage", permissions);
        Assert.Contains("vacancies.manage", permissions);
        Assert.Contains("applications.manage", permissions);
        Assert.Contains("analytics.read", permissions);
        Assert.DoesNotContain("candidates.read", permissions);
        Assert.DoesNotContain("moderation.manage", permissions);
        Assert.DoesNotContain("admin.full_access", permissions);
    }

    [Fact]
    public async Task SeedAsync_AssignsRecruiterPermissions_ForVacanciesApplicationsCandidatesAndCrm()
    {
        await using var db = CreateDbContext();
        await new RoleSeeder(db).SeedAsync();

        var permissions = await GetPermissionCodesAsync(db, "recruiter");

        Assert.Contains("companies.read", permissions);
        Assert.Contains("vacancies.manage", permissions);
        Assert.Contains("applications.manage", permissions);
        Assert.Contains("candidates.read", permissions);
        Assert.Contains("crm.read", permissions);
        Assert.Contains("crm.manage", permissions);
        Assert.DoesNotContain("companies.manage", permissions);
        Assert.DoesNotContain("moderation.manage", permissions);
        Assert.DoesNotContain("admin.full_access", permissions);
    }

    [Fact]
    public async Task SeedAsync_AssignsAdminAllPermissionsAndFullAccess()
    {
        await using var db = CreateDbContext();
        await new RoleSeeder(db).SeedAsync();

        var admin = await db.Set<Role>()
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .SingleAsync(x => x.Code == "admin");
        var allPermissions = await db.Set<Permission>().Select(x => x.Code).ToArrayAsync();
        var adminPermissions = admin.RolePermissions.Select(x => x.Permission.Code).OrderBy(x => x).ToArray();

        Assert.True(admin.IsFullAccess);
        Assert.Equal(allPermissions.OrderBy(x => x), adminPermissions);
        Assert.Contains("moderation.read", adminPermissions);
        Assert.Contains("moderation.manage", adminPermissions);
        Assert.Contains("users.manage", adminPermissions);
        Assert.Contains("admin.full_access", adminPermissions);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_DoesNotDuplicateRolePermissions()
    {
        await using var db = CreateDbContext();
        var seeder = new RoleSeeder(db);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var duplicateAssignments = await db.Set<RolePermission>()
            .GroupBy(x => new { x.RoleId, x.PermissionId })
            .Where(x => x.Count() > 1)
            .ToArrayAsync();

        Assert.Empty(duplicateAssignments);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<string[]> GetPermissionCodesAsync(AppDbContext db, string roleCode)
    {
        var role = await db.Set<Role>()
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .SingleAsync(x => x.Code == roleCode);

        return role.RolePermissions.Select(x => x.Permission.Code).OrderBy(x => x).ToArray();
    }

    private static void AssertRole(Role role, string code, bool isSystem, bool isFullAccess)
    {
        Assert.Equal(code, role.Code);
        Assert.Equal(isSystem, role.IsSystem);
        Assert.Equal(isFullAccess, role.IsFullAccess);
    }
}
