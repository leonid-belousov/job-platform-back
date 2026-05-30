using FluentValidation;
using JobPlatform.Core.Entities.Applications;

namespace JobPlatform.BLL.CQRS.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusCommandValidator : AbstractValidator<ChangeApplicationStatusCommand>
{
    public ChangeApplicationStatusCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(x => ApplicationStatuses.All.Contains(x, StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}