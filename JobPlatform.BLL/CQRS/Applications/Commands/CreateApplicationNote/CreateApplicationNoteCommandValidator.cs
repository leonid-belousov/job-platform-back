using FluentValidation;

namespace JobPlatform.BLL.CQRS.Applications.Commands.CreateApplicationNote;

public sealed class CreateApplicationNoteCommandValidator : AbstractValidator<CreateApplicationNoteCommand>
{
    public CreateApplicationNoteCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
    }
}
