using FluentValidation;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateResume;

public class CreateResumeCommandValidator : AbstractValidator<CreateResumeCommand>
{
    public CreateResumeCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FileId).NotEmpty();
    }
}