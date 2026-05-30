using FluentValidation;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireOption;

public sealed class AddQuestionnaireOptionCommandValidator : AbstractValidator<AddQuestionnaireOptionCommand>
{
    public AddQuestionnaireOptionCommandValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
