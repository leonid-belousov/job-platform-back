using FluentValidation;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.UpdateVacancy;

public sealed class UpdateVacancyCommandValidator : AbstractValidator<UpdateVacancyCommand>
{
    public UpdateVacancyCommandValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.Requirements).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.Responsibilities).MaximumLength(8000);
        RuleFor(x => x.Conditions).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(80);
        RuleFor(x => x.City).MaximumLength(120);
        RuleFor(x => x.EmploymentType).MaximumLength(100);
        RuleFor(x => x.WorkFormat).MaximumLength(100);
        RuleFor(x => x.ExperienceLevel).MaximumLength(100);
        RuleFor(x => x.Currency).MaximumLength(10);
        RuleFor(x => x.SalaryFrom).GreaterThanOrEqualTo(0).When(x => x.SalaryFrom.HasValue);
        RuleFor(x => x.SalaryTo).GreaterThanOrEqualTo(0).When(x => x.SalaryTo.HasValue);
        RuleFor(x => x)
            .Must(x => !x.SalaryFrom.HasValue || !x.SalaryTo.HasValue || x.SalaryFrom <= x.SalaryTo)
            .WithMessage("Зарплата от не может быть больше зарплаты до.");
    }
}