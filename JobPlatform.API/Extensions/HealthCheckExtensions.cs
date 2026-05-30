using System.Text.Json;
using JobPlatform.API.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JobPlatform.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddRecruitmentHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running."), tags: new[] { "live" })
            .AddCheck<PostgresHealthCheck>("postgres", tags: new[] { "ready", "db" });

        return services;
    }

    public static IEndpointRouteBuilder MapRecruitmentHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteJsonResponseAsync
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteJsonResponseAsync
        });

        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteJsonResponseAsync
        });

        return endpoints;
    }

    private static async Task WriteJsonResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
