using FluentValidation;

namespace JobPlatform.BLL.CQRS.Applications.Commands.UpdateApplicationNote;

public sealed class UpdateApplicationNoteCommandValidator : AbstractValidator<UpdateApplicationNoteCommand>
{
    public UpdateApplicationNoteCommandValidator()
    {
        RuleFor(x => x.NoteId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
    }
}
