using FluentValidation;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.ApproveVacancy;

public sealed class ApproveVacancyCommandValidator : AbstractValidator<ApproveVacancyCommand>
{
    public ApproveVacancyCommandValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}
