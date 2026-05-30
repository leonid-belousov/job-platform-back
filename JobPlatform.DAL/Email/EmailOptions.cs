namespace JobPlatform.DAL.Email;

public sealed class EmailOptions
{
    public bool Enabled { get; set; }
    public string From { get; set; } = "noreply@localhost";
}