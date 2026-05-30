using FluentValidation;
using JobPlatform.Core.Entities.Dictionaries;

namespace JobPlatform.BLL.CQRS.Dictionaries.Commands.CreateDictionaryItem;

public sealed class CreateDictionaryItemCommandValidator : AbstractValidator<CreateDictionaryItemCommand>
{
    public CreateDictionaryItemCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(type => DictionaryTypes.All.Contains(type))
            .WithMessage("Unsupported dictionary type.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(120)
            .Matches("^[a-z0-9_.-]+$")
            .WithMessage("Code can contain only lowercase latin letters, numbers, underscore, dot and hyphen.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
