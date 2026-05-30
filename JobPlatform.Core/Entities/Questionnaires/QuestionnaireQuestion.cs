using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Questionnaires;

public sealed class QuestionnaireQuestion : BaseEntity
{
    public Guid SectionId { get; set; }
    public QuestionnaireSection Section { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = QuestionnaireQuestionTypes.Text;
    public bool IsRequired { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? ValidationRules { get; set; }
    public string? VisibilityCondition { get; set; }
    public int SortOrder { get; set; }
    public ICollection<QuestionnaireOption> Options { get; set; } = new List<QuestionnaireOption>();
}
