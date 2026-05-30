using FluentValidation;

namespace JobPlatform.BLL.CQRS.Candidates.Queries.SearchCandidates;

public sealed class SearchCandidatesQueryValidator : AbstractValidator<SearchCandidatesQuery>
{
    private static readonly string[] AllowedSorts = { "date", "profession", "country" };

    public SearchCandidatesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Text).MaximumLength(200);
        RuleFor(x => x.Skill).MaximumLength(100);
        RuleFor(x => x.ExperienceLevel).MaximumLength(50);
        RuleFor(x => x.Country).MaximumLength(80);
        RuleFor(x => x.Language).MaximumLength(20);
        RuleFor(x => x.Profession).MaximumLength(200);
        RuleFor(x => x.JobSearchStatus).MaximumLength(50);
        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSorts.Contains(x, StringComparer.OrdinalIgnoreCase));
    }
}