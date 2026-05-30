using JobPlatform.DAL.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JobPlatform.API.HealthChecks;

public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public PostgresHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("PostgreSQL connection is available.")
                : HealthCheckResult.Unhealthy("PostgreSQL connection is not available.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL health check failed.", exception);
        }
    }
}