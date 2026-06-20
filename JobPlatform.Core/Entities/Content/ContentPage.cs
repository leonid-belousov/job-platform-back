using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Content;

public sealed class ContentPage : BaseEntity
{
    public string Slug { get; set; } = string.Empty;
    public int SchemaVersion { get; set; } = 1;
    public string JsonContent { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
