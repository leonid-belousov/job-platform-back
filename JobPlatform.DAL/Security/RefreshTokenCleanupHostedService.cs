using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobPlatform.DAL.Security;

public sealed class RefreshTokenCleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupHostedService> _logger;
    private readonly RefreshTokenCleanupOptions _options;

    public RefreshTokenCleanupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupHostedService> logger,
        IOptions<RefreshTokenCleanupOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(Math.Max(1, _options.CleanupIntervalHours));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token cleanup failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var threshold = DateTimeOffset.UtcNow.AddDays(-Math.Max(1, _options.RetentionDaysAfterExpiration));

        var expiredTokens = await db.Set<RefreshToken>()
            .Where(x => x.ExpiresAt < threshold && x.RevokedAt != null)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Count == 0)
            return;

        db.Set<RefreshToken>().RemoveRange(expiredTokens);
        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed {Count} old refresh tokens.", expiredTokens.Count);
    }
}