using FluentValidation;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.UpdateQuestionnaire;

public sealed class UpdateQuestionnaireCommandValidator : AbstractValidator<UpdateQuestionnaireCommand>
{
    public UpdateQuestionnaireCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}
