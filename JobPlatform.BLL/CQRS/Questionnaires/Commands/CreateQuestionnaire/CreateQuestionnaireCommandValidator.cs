using FluentValidation;
using JobPlatform.Core.Entities.Questionnaires;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.CreateQuestionnaire;

public sealed class CreateQuestionnaireCommandValidator : AbstractValidator<CreateQuestionnaireCommand>
{
    public CreateQuestionnaireCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.EntityType).NotEmpty().Must(x => QuestionnaireEntityTypes.All.Contains(x)).WithMessage("Недопустимый тип сущности для анкеты.");
    }
}
