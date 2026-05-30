using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Questionnaires;

public sealed class Questionnaire : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByUserId { get; set; }
    public ICollection<QuestionnaireSection> Sections { get; set; } = new List<QuestionnaireSection>();
    public ICollection<QuestionnaireResponse> Responses { get; set; } = new List<QuestionnaireResponse>();
}
