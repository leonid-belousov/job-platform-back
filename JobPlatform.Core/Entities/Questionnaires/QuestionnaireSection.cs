using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Questionnaires;

public sealed class QuestionnaireSection : BaseEntity
{
    public Guid QuestionnaireId { get; set; }
    public Questionnaire Questionnaire { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ICollection<QuestionnaireQuestion> Questions { get; set; } = new List<QuestionnaireQuestion>();
}