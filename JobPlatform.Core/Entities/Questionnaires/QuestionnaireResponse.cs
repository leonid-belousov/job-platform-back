using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Questionnaires;

public sealed class QuestionnaireResponse : BaseEntity
{
    public Guid QuestionnaireId { get; set; }
    public Questionnaire Questionnaire { get; set; } = null!;
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public ICollection<QuestionnaireAnswer> Answers { get; set; } = new List<QuestionnaireAnswer>();
}
