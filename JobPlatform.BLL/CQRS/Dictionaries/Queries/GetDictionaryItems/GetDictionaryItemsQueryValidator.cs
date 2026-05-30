using FluentValidation;
using JobPlatform.Core.Entities.Dictionaries;

namespace JobPlatform.BLL.CQRS.Dictionaries.Queries.GetDictionaryItems;

public sealed class GetDictionaryItemsQueryValidator : AbstractValidator<GetDictionaryItemsQuery>
{
    public GetDictionaryItemsQueryValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(type => DictionaryTypes.All.Contains(type))
            .WithMessage("Unsupported dictionary type.");

        RuleFor(x => x.Search).MaximumLength(200);
    }
}