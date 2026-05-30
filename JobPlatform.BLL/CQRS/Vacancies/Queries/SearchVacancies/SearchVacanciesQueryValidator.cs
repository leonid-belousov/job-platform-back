using FluentValidation;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.SearchVacancies;

public sealed class SearchVacanciesQueryValidator : AbstractValidator<SearchVacanciesQuery>
{
    private static readonly string[] AllowedSorts = { "date", "salary", "relevance" };

    public SearchVacanciesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Text).MaximumLength(200);
        RuleFor(x => x.Country).MaximumLength(80);
        RuleFor(x => x.City).MaximumLength(120);
        RuleFor(x => x.SalaryFrom).GreaterThanOrEqualTo(0).When(x => x.SalaryFrom.HasValue);
        RuleFor(x => x.SalaryTo).GreaterThanOrEqualTo(0).When(x => x.SalaryTo.HasValue);
        RuleFor(x => x)
            .Must(x => !x.SalaryFrom.HasValue || !x.SalaryTo.HasValue || x.SalaryFrom <= x.SalaryTo);
        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSorts.Contains(x, StringComparer.OrdinalIgnoreCase));
    }
}