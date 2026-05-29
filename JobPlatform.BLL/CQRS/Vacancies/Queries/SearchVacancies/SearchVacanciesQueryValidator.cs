using FluentValidation;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.SearchVacancies;

public sealed class SearchVacanciesQueryValidator : AbstractValidator<SearchVacanciesQuery>
{
    public SearchVacanciesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Text).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(120);
    }
}