using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.CRM;

public sealed class CrmTask : BaseEntity
{
    public Guid LeadId { get; set; }
    public CrmLead Lead { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string Status { get; set; } = CrmTaskStatuses.Open;
    public Guid? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? CompletedByUserId { get; set; }
}
