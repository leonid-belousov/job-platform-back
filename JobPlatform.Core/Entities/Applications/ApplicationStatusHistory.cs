using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Applications;

public sealed class ApplicationStatusHistory : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedByUserId { get; set; }
    public string? Comment { get; set; }
}