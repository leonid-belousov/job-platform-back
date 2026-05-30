using FluentValidation;

namespace JobPlatform.BLL.CQRS.Dictionaries.Commands.UpdateDictionaryItem;

public sealed class UpdateDictionaryItemCommandValidator : AbstractValidator<UpdateDictionaryItemCommand>
{
    public UpdateDictionaryItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(120)
            .Matches("^[a-z0-9_.-]+$")
            .WithMessage("Code can contain only lowercase latin letters, numbers, underscore, dot and hyphen.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}