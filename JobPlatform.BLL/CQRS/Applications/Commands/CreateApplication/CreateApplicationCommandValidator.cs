using FluentValidation;

namespace JobPlatform.BLL.CQRS.Applications.Commands.CreateApplication;

public sealed class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.ResumeId).NotEmpty();
        RuleFor(x => x.CoverLetter).MaximumLength(4000);
    }
}