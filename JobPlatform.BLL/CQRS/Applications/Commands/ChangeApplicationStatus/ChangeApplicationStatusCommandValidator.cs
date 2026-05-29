using FluentValidation;

namespace JobPlatform.BLL.CQRS.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusCommandValidator : AbstractValidator<ChangeApplicationStatusCommand>
{
    private static readonly string[] AllowedStatuses = { "Sent", "Viewed", "InProgress", "Interview", "Rejected", "Accepted", "Closed" };

    public ChangeApplicationStatusCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(x => AllowedStatuses.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Недопустимый статус отклика.");
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}
