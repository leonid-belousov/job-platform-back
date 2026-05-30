using FluentValidation;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireSection;

public sealed class AddQuestionnaireSectionCommandValidator : AbstractValidator<AddQuestionnaireSectionCommand>
{
    public AddQuestionnaireSectionCommandValidator()
    {
        RuleFor(x => x.QuestionnaireId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}