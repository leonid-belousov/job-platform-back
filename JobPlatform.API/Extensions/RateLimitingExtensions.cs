using System.Net;
using System.Threading.RateLimiting;
using JobPlatform.API.Options;

namespace JobPlatform.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicyName = "auth-rate-limit";

    public static IServiceCollection AddRecruitmentRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(AuthRateLimitOptions.SectionName).Get<AuthRateLimitOptions>() ?? new AuthRateLimitOptions();

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddPolicy(AuthPolicyName, context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var path = context.Request.Path.ToString().ToLowerInvariant();
                var partitionKey = $"{ipAddress}:{path}";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    QueueLimit = options.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
            });

            rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                const string payload = "{\"status\":429,\"title\":\"Too many requests\",\"detail\":\"Too many authentication requests. Try again later.\"}";
                await context.HttpContext.Response.WriteAsync(payload, cancellationToken);
            };
        });

        return services;
    }
}
