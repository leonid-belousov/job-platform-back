using JobPlatform.Core.Entities.Common;
using JobPlatform.Core.Entities.Users;

namespace JobPlatform.Core.Entities.CRM;

public sealed class CrmLead : BaseEntity
{
    public string Type { get; set; } = CrmLeadTypes.Candidate;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = CrmLeadStatuses.New;
    public string? Source { get; set; }
    public string? Description { get; set; }
    public Guid? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    public Guid? CandidateProfileId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? VacancyId { get; set; }
    public Guid? ApplicationId { get; set; }
    public ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
    public ICollection<CrmActivity> Activities { get; set; } = new List<CrmActivity>();
}