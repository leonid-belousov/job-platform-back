using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.DAL.Seeds;

public class RoleSeeder
{
    private readonly AppDbContext _db;

    public RoleSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRoleDefinitions(cancellationToken);
        await SeedPermissionDefinitions(cancellationToken);
    }

    private async Task SeedRoleDefinitions(CancellationToken cancellationToken)
    {
        var roleDefinitions = new[]
        {
            new Role { Code = "candidate", Name = "Candidate", IsSystem = true },
            new Role { Code = "employer", Name = "Employer", IsSystem = true },
            new Role { Code = "recruiter", Name = "Recruiter", IsSystem = true },
            new Role { Code = "admin", Name = "Admin", IsSystem = true, IsFullAccess = true }
        };

        foreach (var roleDefinition in roleDefinitions)
        {
            var existingRole = await _db.Set<Role>()
                .FirstOrDefaultAsync(x => x.Code == roleDefinition.Code, cancellationToken);
            if (existingRole is null)
            {
                _db.Set<Role>().Add(roleDefinition);
            }
            else
            {
                existingRole.Name = roleDefinition.Name;
                existingRole.IsSystem = roleDefinition.IsSystem;
                existingRole.IsFullAccess = roleDefinition.IsFullAccess;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionDefinitions(CancellationToken cancellationToken = default)
    {
        var permissionDefinitions = new[]
        {
            new Permission { Code = "users.read", Name = "Read users", Module = "Users" },
            new Permission { Code = "users.manage", Name = "Manage users", Module = "Users" },
            new Permission { Code = "companies.read", Name = "Read companies", Module = "Companies" },
            new Permission { Code = "companies.manage", Name = "Manage companies", Module = "Companies" },
            new Permission { Code = "vacancies.read", Name = "Read vacancies", Module = "Vacancies" },
            new Permission { Code = "vacancies.manage", Name = "Manage vacancies", Module = "Vacancies" },
            new Permission { Code = "applications.read", Name = "Read applications", Module = "Applications" },
            new Permission { Code = "applications.manage", Name = "Manage applications", Module = "Applications" },
            new Permission { Code = "candidates.read", Name = "Read candidates", Module = "Candidates" },
            new Permission { Code = "candidates.manage_own", Name = "Manage own candidate profile", Module = "Candidates" },
            new Permission { Code = "dictionaries.read", Name = "Read dictionaries", Module = "Dictionaries" },
            new Permission { Code = "dictionaries.manage", Name = "Manage dictionaries", Module = "Dictionaries" },
            new Permission { Code = "moderation.read", Name = "Read moderation queue", Module = "Moderation" },
            new Permission { Code = "moderation.manage", Name = "Manage moderation", Module = "Moderation" },
            new Permission { Code = "crm.read", Name = "Read CRM", Module = "CRM" },
            new Permission { Code = "crm.manage", Name = "Manage CRM", Module = "CRM" },
            new Permission { Code = "questionnaires.read", Name = "Read questionnaires", Module = "Questionnaires" },
            new Permission { Code = "questionnaires.manage", Name = "Manage questionnaires", Module = "Questionnaires" },
            new Permission { Code = "analytics.read", Name = "Read analytics", Module = "Analytics" },
            new Permission { Code = "admin.full_access", Name = "Full admin access", Module = "Admin" }
        };

        foreach (var permissionDefinition in permissionDefinitions)
        {
            var existingPermission = await _db.Set<Permission>()
                .FirstOrDefaultAsync(x => x.Code == permissionDefinition.Code, cancellationToken);
            if (existingPermission is null)
            {
                _db.Set<Permission>().Add(permissionDefinition);
            }
            else
            {
                existingPermission.Name = permissionDefinition.Name;
                existingPermission.Module = permissionDefinition.Module;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await AssignAsync("candidate",
            new[]
            {
                "candidates.manage_own", "vacancies.read", "applications.read", "dictionaries.read",
                "questionnaires.read"
            },
            cancellationToken);
        await AssignAsync("employer",
            new[]
            {
                "companies.manage", "vacancies.manage", "applications.manage", "dictionaries.read",
                "questionnaires.read", "analytics.read"
            }, cancellationToken);
        await AssignAsync("recruiter",
            new[]
            {
                "companies.read", "vacancies.manage", "applications.manage", "candidates.read", "dictionaries.read", "crm.read",
                "crm.manage", "questionnaires.read", "questionnaires.manage", "analytics.read"
            },
            cancellationToken);
        await AssignAsync("admin", permissionDefinitions.Select(x => x.Code).ToArray(), cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task AssignAsync(string roleCode, IReadOnlyCollection<string> permissionCodes,
        CancellationToken cancellationToken)
    {
        var role = await _db.Set<Role>().Include(x => x.RolePermissions)
            .FirstAsync(x => x.Code == roleCode, cancellationToken);
        var permissions = await _db.Set<Permission>().Where(x => permissionCodes.Contains(x.Code))
            .ToArrayAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            if (role.RolePermissions.All(x => x.PermissionId != permission.Id))
            {
                role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }
    }
}