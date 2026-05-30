using FluentValidation;
using JobPlatform.Core.Entities.Questionnaires;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireQuestion;

public sealed class AddQuestionnaireQuestionCommandValidator : AbstractValidator<AddQuestionnaireQuestionCommand>
{
    public AddQuestionnaireQuestionCommandValidator()
    {
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.QuestionType).NotEmpty().Must(x => QuestionnaireQuestionTypes.All.Contains(x)).WithMessage("Недопустимый тип вопроса.");
        RuleFor(x => x.Placeholder).MaximumLength(500);
        RuleFor(x => x.HelpText).MaximumLength(1000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
