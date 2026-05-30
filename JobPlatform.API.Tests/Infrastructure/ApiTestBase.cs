namespace JobPlatform.API.Tests.Infrastructure;

public abstract class ApiTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected ApiTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    protected CustomWebApplicationFactory Factory { get; }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
