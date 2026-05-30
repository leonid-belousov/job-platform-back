using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Questionnaires;

public sealed class QuestionnaireOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public QuestionnaireQuestion Question { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
