using FluentValidation;

namespace JobPlatform.BLL.CQRS.Dictionaries.Commands.SetDictionaryItemActive;

public sealed class SetDictionaryItemActiveCommandValidator : AbstractValidator<SetDictionaryItemActiveCommand>
{
    public SetDictionaryItemActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
