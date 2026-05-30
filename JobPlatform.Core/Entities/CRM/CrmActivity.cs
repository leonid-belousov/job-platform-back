using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.CRM;

public sealed class CrmActivity : BaseEntity
{
    public Guid LeadId { get; set; }
    public CrmLead Lead { get; set; } = null!;
    public string Type { get; set; } = CrmActivityTypes.Note;
    public string Description { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}
