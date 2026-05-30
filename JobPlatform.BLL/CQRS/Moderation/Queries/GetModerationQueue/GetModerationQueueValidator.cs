using FluentValidation;

namespace JobPlatform.BLL.CQRS.Moderation.Queries.GetModerationQueue;

public sealed class GetModerationQueueValidator : AbstractValidator<GetModerationQueueQuery>
{
    public GetModerationQueueValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.EntityType).Must(x => string.IsNullOrWhiteSpace(x) || x is "company" or "vacancy")
            .WithMessage("EntityType must be 'company' or 'vacancy'.");
    }
}