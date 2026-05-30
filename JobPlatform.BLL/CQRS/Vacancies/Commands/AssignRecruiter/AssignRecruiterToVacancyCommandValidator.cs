using FluentValidation;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.AssignRecruiter;

public sealed class AssignRecruiterToVacancyCommandValidator : AbstractValidator<AssignRecruiterToVacancyCommand>
{
    public AssignRecruiterToVacancyCommandValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.RecruiterUserId).NotEmpty();
    }
}