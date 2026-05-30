using JobPlatform.Core.Entities.CRM;

namespace JobPlatform.BLL.CQRS.CRM.DTO;

public static class CrmMappings
{
    public static CrmLeadDto ToDto(this CrmLead lead) => new(
        lead.Id,
        lead.Type,
        lead.Name,
        lead.Status,
        lead.Source,
        lead.Description,
        lead.ResponsibleUserId,
        lead.CandidateProfileId,
        lead.CompanyId,
        lead.VacancyId,
        lead.ApplicationId,
        lead.CreatedAt,
        lead.UpdatedAt);

    public static CrmTaskDto ToDto(this CrmTask task) => new(
        task.Id,
        task.LeadId,
        task.Title,
        task.Description,
        task.DueDate,
        task.Status,
        task.ResponsibleUserId,
        task.CreatedByUserId,
        task.CompletedAt,
        task.CompletedByUserId,
        task.CreatedAt);

    public static CrmActivityDto ToDto(this CrmActivity activity) => new(
        activity.Id,
        activity.LeadId,
        activity.Type,
        activity.Description,
        activity.CreatedByUserId,
        activity.RelatedEntityId,
        activity.RelatedEntityType,
        activity.CreatedAt);
}
