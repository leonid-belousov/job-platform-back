namespace JobPlatform.API.Middleware;

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRecruitmentRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
