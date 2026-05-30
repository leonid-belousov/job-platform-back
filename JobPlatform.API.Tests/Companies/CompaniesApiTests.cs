using System.Net;
using System.Net.Http.Json;
using JobPlatform.API.Tests.Infrastructure;

namespace JobPlatform.API.Tests.Companies;

public sealed class CompaniesApiTests : ApiTestBase
{
    public CompaniesApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateCompany_AsEmployer_ReturnsCompany()
    {
        var token = await Factory.RegisterAndGetTokenAsync("employer");
        var client = Factory.CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            name = "Acme Recruitment",
            description = "Employer company",
            industry = "it",
            website = "https://example.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var company = await response.Content.ReadAsAsync<CompanyPayload>();
        Assert.Equal("Acme Recruitment", company.Name);
        Assert.Equal("Pending", company.ModerationStatus);
    }

    [Fact]
    public async Task CreateCompany_AsCandidate_ReturnsForbidden()
    {
        var token = await Factory.RegisterAndGetTokenAsync("candidate");
        var client = Factory.CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            name = "Candidate Company",
            description = "Should not be allowed",
            industry = "it",
            website = "https://example.com"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record CompanyPayload(Guid Id, string Name, string? ModerationStatus);
}
