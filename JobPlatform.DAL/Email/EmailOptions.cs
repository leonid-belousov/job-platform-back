namespace JobPlatform.DAL.Email;

public sealed class EmailOptions
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Smtp";
    public string FromEmail { get; set; } = "noreply@localhost";
    public string FromName { get; set; } = "Recruitment Platform";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool EnableSsl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}
