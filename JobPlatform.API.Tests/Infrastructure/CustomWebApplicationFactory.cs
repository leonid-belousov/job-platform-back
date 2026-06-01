using System.Net.Http.Headers;
using System.Net.Http.Json;
using JobPlatform.Core.Entities.Legal;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Context;
using JobPlatform.DAL.Interfaces;
using JobPlatform.DAL.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace JobPlatform.API.Tests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestLegalDocumentVersion = "test-v1";

    private readonly string _databaseName = $"recruitment-api-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Database=recruitment_tests;Username=test;Password=test",
                ["Jwt:Issuer"] = "RecruitmentPlatform",
                ["Jwt:Audience"] = "RecruitmentPlatform.Web",
                ["Jwt:Secret"] = "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_MIN_32_CHARS",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Email:Enabled"] = "false",
                ["RateLimiting:Auth:PermitLimit"] = "1000",
                ["RateLimiting:Auth:WindowSeconds"] = "60",
                ["RateLimiting:Auth:QueueLimit"] = "0",
                ["FileStorage:RootPath"] = Path.Combine(Path.GetTempPath(), "recruitment-platform-test-files",
                    Guid.NewGuid().ToString("N"))
            };

            configuration.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IApplicationDbContext>();
            services.RemoveAll<IHostedService>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<RoleSeeder>();
            services.AddScoped<DictionarySeeder>();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
        var dictionarySeeder = scope.ServiceProvider.GetRequiredService<DictionarySeeder>();
        
        await roleSeeder.SeedAsync();
        await dictionarySeeder.SeedAsync();
        await SeedLegalDocumentsAsync(db);
    }

    public async Task<string> RegisterAndGetTokenAsync(string roleCode, string? email = null)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = email ?? $"{roleCode}-{Guid.NewGuid():N}@example.com",
            password = "Password123",
            roleCode,
            acceptTerms = true,
            termsVersion = TestLegalDocumentVersion,
            acceptPrivacyPolicy = true,
            privacyPolicyVersion = TestLegalDocumentVersion,
            language = "ru"
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponsePayload>()
                      ?? throw new InvalidOperationException("Auth response is empty.");

        return payload.AccessToken;
    }

    public HttpClient CreateAuthenticatedClient(string accessToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public async Task<string> CreateAdminTokenAsync(string? email = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var role = await db.Set<Role>()
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .FirstAsync(x => x.Code == "admin");

        var user = new User
        {
            Email = email ?? $"admin-{Guid.NewGuid():N}@example.com",
            Status = UserStatus.Active,
            EmailConfirmed = true
        };
        user.PasswordHash = passwordService.HashPassword(user, "Password123");
        user.UserRoles.Add(new UserRole { User = user, Role = role });

        await db.Set<User>().AddAsync(user);
        await db.SaveChangesAsync();

        var permissions = role.RolePermissions.Select(x => x.Permission.Code).Distinct().ToArray();
        return jwtTokenService.CreateAccessToken(user, new[] { role.Code }, permissions);
    }

    private static async Task SeedLegalDocumentsAsync(AppDbContext db)
    {
        await db.Set<LegalDocument>().AddRangeAsync(
            new LegalDocument
            {
                Type = "terms",
                Version = TestLegalDocumentVersion,
                Language = "ru",
                Title = "Test terms of service",
                Content = "Test terms of service content.",
                IsActive = true,
                PublishedAt = DateTimeOffset.UtcNow
            },
            new LegalDocument
            {
                Type = LegalDocumentTypes.PrivacyPolicy,
                Version = TestLegalDocumentVersion,
                Language = "ru",
                Title = "Test privacy policy",
                Content = "Test privacy policy content.",
                IsActive = true,
                PublishedAt = DateTimeOffset.UtcNow
            });

        await db.SaveChangesAsync();
    }

    private sealed record AuthResponsePayload(string AccessToken, string RefreshToken, Guid UserId, string Email);
}