using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Legal;

public sealed class LegalDocument : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Language { get; set; } = "ru";
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;
}