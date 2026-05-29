using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Files;

public sealed class StoredFile : BaseEntity
{
    public string OriginalName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
}