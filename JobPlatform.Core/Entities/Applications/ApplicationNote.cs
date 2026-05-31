using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.Applications;

public sealed class ApplicationNote : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;
    public Guid AuthorUserId { get; set; }
    public User AuthorUser { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
}