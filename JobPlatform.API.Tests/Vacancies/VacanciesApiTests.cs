using System.Net;
using System.Net.Http.Json;
using JobPlatform.API.Tests.Infrastructure;

namespace JobPlatform.API.Tests.Vacancies;

public sealed class VacanciesApiTests : ApiTestBase
{
    public VacanciesApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateVacancy_ForOwnCompany_ReturnsVacancy()
    {
        var token = await Factory.RegisterAndGetTokenAsync("employer");
        var client = Factory.CreateAuthenticatedClient(token);
        var company = await CreateCompanyAsync(client);

        var response = await client.PostAsJsonAsync("/api/vacancies", new
        {
            companyId = company.Id,
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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vacancy = await response.Content.ReadAsAsync<VacancyPayload>();
        Assert.Equal("C# Backend Developer", vacancy.Title);
    }

    [Fact]
    public async Task PublishVacancy_BeforeModeration_MovesVacancyToPendingModeration()
    {
        var token = await Factory.RegisterAndGetTokenAsync("employer");
        var client = Factory.CreateAuthenticatedClient(token);
        var company = await CreateCompanyAsync(client);
        var vacancy = await CreateVacancyAsync(client, company.Id);

        var response = await client.PostAsync($"/api/vacancies/{vacancy.Id}/publish", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var published = await response.Content.ReadAsAsync<VacancyPayload>();
        Assert.Equal("PendingModeration", published.Status);
        Assert.Equal("Pending", published.ModerationStatus);
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
    private sealed record VacancyPayload(Guid Id, Guid CompanyId, string Title, string Status, string? ModerationStatus);
}
