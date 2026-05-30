using JobPlatform.DAL.Context;
using JobPlatform.DAL.Email;
using JobPlatform.DAL.Files;
using JobPlatform.DAL.Interfaces;
using JobPlatform.DAL.Security;
using JobPlatform.DAL.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace JobPlatform.DAL;

public static class ServiceCollectionExtensions
{
    public static void AddDALServiceCollection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(p =>
        {
            p.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(60);
                npgsqlOptions.MaxBatchSize(50);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<IApplicationDbContext>(p => p.GetService<AppDbContext>()!);
        services.AddScoped<RoleSeeder>();
        services.AddScoped<DictionarySeeder>();
        services.Configure<MinioStorageOptions>(configuration.GetSection("Minio"));
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.Configure<RefreshTokenCleanupOptions>(configuration.GetSection("RefreshTokenCleanup"));
        services.AddHostedService<RefreshTokenCleanupHostedService>();
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
    }
}