using FluentValidation;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.ExtendVacancy;

public sealed class ExtendVacancyCommandValidator : AbstractValidator<ExtendVacancyCommand>
{
    public ExtendVacancyCommandValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.Days).InclusiveBetween(1, 90);
    }
}