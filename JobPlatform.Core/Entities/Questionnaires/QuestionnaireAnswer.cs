using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Questionnaires;

public sealed class QuestionnaireAnswer : BaseEntity
{
    public Guid ResponseId { get; set; }
    public QuestionnaireResponse Response { get; set; } = null!;
    public Guid QuestionId { get; set; }
    public QuestionnaireQuestion Question { get; set; } = null!;
    public string? Value { get; set; }
    public Guid? FileId { get; set; }
}
