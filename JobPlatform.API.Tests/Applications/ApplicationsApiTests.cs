using System.Net;
using System.Net.Http.Json;
using JobPlatform.API.Tests.Infrastructure;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobPlatform.API.Tests.Applications;

public sealed class ApplicationsApiTests : ApiTestBase
{
    public ApplicationsApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateApplication_AsCandidate_ReturnsApplication()
    {
        var employerToken = await Factory.RegisterAndGetTokenAsync("employer");
        var employerClient = Factory.CreateAuthenticatedClient(employerToken);
        var company = await CreateCompanyAsync(employerClient);
        var vacancy = await CreateVacancyAsync(employerClient, company.Id);
        await ApproveAndPublishVacancyDirectlyAsync(vacancy.Id);

        var candidateToken = await Factory.RegisterAndGetTokenAsync("candidate");
        var candidateClient = Factory.CreateAuthenticatedClient(candidateToken);
        await candidateClient.PostAsJsonAsync("/api/candidates/profile", new
        {
            firstName = "Ivan",
            lastName = "Ivanov",
            city = "moscow",
            desiredPosition = "C# Backend Developer",
            expectedSalary = 200000,
            about = "Backend candidate"
        });
        var resumeResponse = await candidateClient.PostAsJsonAsync("/api/candidates/me/resumes", new
        {
            title = "Main resume",
            fileId = (Guid?)null,
            isDefault = true
        });
        resumeResponse.EnsureSuccessStatusCode();
        var resume = await resumeResponse.Content.ReadAsAsync<ResumePayload>();

        var applicationResponse = await candidateClient.PostAsJsonAsync("/api/applications", new
        {
            vacancyId = vacancy.Id,
            resumeId = resume.Id,
            coverLetter = "Interested in this vacancy."
        });
        Assert.Equal(HttpStatusCode.OK, applicationResponse.StatusCode);
        var application = await applicationResponse.Content.ReadAsAsync<ApplicationPayload>();
        Assert.Equal(vacancy.Id, application.VacancyId);
        Assert.Equal("Sent", application.Status);
    }

    private async Task ApproveAndPublishVacancyDirectlyAsync(Guid vacancyId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var vacancy = await db.Set<JobVacancy>().FirstAsync(x => x.Id == vacancyId);
        vacancy.ModerationStatus = ModerationStatuses.Approved;
        vacancy.Status = "Published";
        vacancy.PublishedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task<CompanyPayload> CreateCompanyAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            name = $"Company {Guid.NewGuid():N}",
            description = "Employer company",
            industry = "it",
            website = "https://example.com"
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<CompanyPayload>();
    }

    private static async Task<VacancyPayload> CreateVacancyAsync(HttpClient client, Guid companyId)
    {
        var response = await client.PostAsJsonAsync("/api/vacancies", new
        {
            companyId,
            title = "C# Backend Developer",
            description = "ASP.NET Core and PostgreSQL development",
            city = "moscow",
            employmentType = "full_time",
            workFormat = "remote",
            experienceLevel = "middle",
            salaryFrom = 150000,
            salaryTo = 250000,
            currency = "rub"
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<VacancyPayload>();
    }

    private sealed record CompanyPayload(Guid Id, string Name);
    private sealed record VacancyPayload(Guid Id, Guid CompanyId, string Title);
    private sealed record ResumePayload(Guid Id, string Title);
    private sealed record ApplicationPayload(Guid Id, Guid VacancyId, Guid ResumeId, string Status);
}
