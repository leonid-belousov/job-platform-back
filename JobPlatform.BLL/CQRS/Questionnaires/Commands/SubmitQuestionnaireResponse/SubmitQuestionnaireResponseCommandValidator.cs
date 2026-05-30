using FluentValidation;
using JobPlatform.Core.Entities.Questionnaires;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.SubmitQuestionnaireResponse;

public sealed class SubmitQuestionnaireResponseCommandValidator : AbstractValidator<SubmitQuestionnaireResponseCommand>
{
    public SubmitQuestionnaireResponseCommandValidator()
    {
        RuleFor(x => x.QuestionnaireId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().Must(x => QuestionnaireEntityTypes.All.Contains(x))
            .WithMessage("Недопустимый тип сущности для ответа анкеты.");
        RuleFor(x => x.Answers).NotNull();
    }
}