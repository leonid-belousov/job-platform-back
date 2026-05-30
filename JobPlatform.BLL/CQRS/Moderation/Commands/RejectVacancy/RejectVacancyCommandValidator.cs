using FluentValidation;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.RejectVacancy;

public sealed class RejectVacancyCommandValidator : AbstractValidator<RejectVacancyCommand>
{
    public RejectVacancyCommandValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}
