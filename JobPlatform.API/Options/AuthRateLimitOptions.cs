namespace JobPlatform.API.Options;

public sealed class AuthRateLimitOptions
{
    public const string SectionName = "RateLimiting:Auth";

    public int PermitLimit { get; set; } = 10;

    public int WindowSeconds { get; set; } = 60;

    public int QueueLimit { get; set; } = 0;
}
