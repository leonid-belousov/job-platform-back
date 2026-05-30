
using JobPlatform.API.Options;

namespace JobPlatform.API.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "RecruitmentCors";

    public static IServiceCollection AddRecruitmentCors(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(cors =>
        {
            cors.AddPolicy(PolicyName, policy =>
            {
                if (options.AllowedOrigins.Length == 0 || options.AllowedOrigins.Contains("*"))
                {
                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(options.AllowedOrigins);
                }

                policy.AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
