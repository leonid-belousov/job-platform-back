using JobPlatform.Core.Entities.Questionnaires;

namespace JobPlatform.BLL.CQRS.Questionnaires.DTO;

public sealed record QuestionnaireDto(
    Guid Id,
    string Title,
    string? Description,
    string EntityType,
    Guid? EntityId,
    bool IsActive,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<QuestionnaireSectionDto> Sections);

public sealed record QuestionnaireListItemDto(
    Guid Id,
    string Title,
    string? Description,
    string EntityType,
    Guid? EntityId,
    bool IsActive,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    int SectionsCount,
    int ResponsesCount);

public sealed record QuestionnaireSectionDto(
    Guid Id,
    string Title,
    string? Description,
    int SortOrder,
    IReadOnlyCollection<QuestionnaireQuestionDto> Questions);

public sealed record QuestionnaireQuestionDto(
    Guid Id,
    string Text,
    string QuestionType,
    bool IsRequired,
    string? Placeholder,
    string? HelpText,
    string? ValidationRules,
    string? VisibilityCondition,
    int SortOrder,
    IReadOnlyCollection<QuestionnaireOptionDto> Options);

public sealed record QuestionnaireOptionDto(Guid Id, string Text, string Value, int SortOrder);

public sealed record QuestionnaireResponseDto(
    Guid Id,
    Guid QuestionnaireId,
    Guid UserId,
    string EntityType,
    Guid? EntityId,
    DateTimeOffset SubmittedAt,
    IReadOnlyCollection<QuestionnaireAnswerDto> Answers);

public sealed record QuestionnaireAnswerDto(Guid Id, Guid QuestionId, string QuestionText, string QuestionType, string? Value, Guid? FileId);

public sealed record SubmitQuestionnaireAnswerDto(Guid QuestionId, string? Value, Guid? FileId);

public static class QuestionnaireMappings
{
    public static QuestionnaireDto ToDto(this Questionnaire questionnaire)
        => new(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.EntityType,
            questionnaire.EntityId,
            questionnaire.IsActive,
            questionnaire.CreatedByUserId,
            questionnaire.CreatedAt,
            questionnaire.UpdatedAt,
            questionnaire.Sections.OrderBy(x => x.SortOrder).ThenBy(x => x.CreatedAt).Select(x => x.ToDto()).ToArray());

    public static QuestionnaireListItemDto ToListItemDto(this Questionnaire questionnaire)
        => new(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.EntityType,
            questionnaire.EntityId,
            questionnaire.IsActive,
            questionnaire.CreatedByUserId,
            questionnaire.CreatedAt,
            questionnaire.UpdatedAt,
            questionnaire.Sections.Count,
            questionnaire.Responses.Count);

    public static QuestionnaireSectionDto ToDto(this QuestionnaireSection section)
        => new(
            section.Id,
            section.Title,
            section.Description,
            section.SortOrder,
            section.Questions.OrderBy(x => x.SortOrder).ThenBy(x => x.CreatedAt).Select(x => x.ToDto()).ToArray());

    public static QuestionnaireQuestionDto ToDto(this QuestionnaireQuestion question)
        => new(
            question.Id,
            question.Text,
            question.QuestionType,
            question.IsRequired,
            question.Placeholder,
            question.HelpText,
            question.ValidationRules,
            question.VisibilityCondition,
            question.SortOrder,
            question.Options.OrderBy(x => x.SortOrder).ThenBy(x => x.CreatedAt).Select(x => x.ToDto()).ToArray());

    public static QuestionnaireOptionDto ToDto(this QuestionnaireOption option)
        => new(option.Id, option.Text, option.Value, option.SortOrder);

    public static QuestionnaireResponseDto ToDto(this QuestionnaireResponse response)
        => new(
            response.Id,
            response.QuestionnaireId,
            response.UserId,
            response.EntityType,
            response.EntityId,
            response.SubmittedAt,
            response.Answers.Select(x => x.ToDto()).ToArray());

    public static QuestionnaireAnswerDto ToDto(this QuestionnaireAnswer answer)
        => new(answer.Id, answer.QuestionId, answer.Question.Text, answer.Question.QuestionType, answer.Value, answer.FileId);
}
