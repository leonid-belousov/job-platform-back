namespace JobPlatform.DAL.Security;

public sealed class RefreshTokenCleanupOptions
{
    public int CleanupIntervalHours { get; set; } = 24;
    public int RetentionDaysAfterExpiration { get; set; } = 30;
}