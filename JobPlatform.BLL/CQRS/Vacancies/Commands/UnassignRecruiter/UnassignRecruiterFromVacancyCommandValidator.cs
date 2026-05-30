using FluentValidation;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.UnassignRecruiter;

public sealed class UnassignRecruiterFromVacancyCommandValidator : AbstractValidator<UnassignRecruiterFromVacancyCommand>
{
    public UnassignRecruiterFromVacancyCommandValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.RecruiterUserId).NotEmpty();
    }
}