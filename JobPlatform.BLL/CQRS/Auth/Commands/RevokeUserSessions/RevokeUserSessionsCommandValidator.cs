using FluentValidation;

namespace JobPlatform.BLL.CQRS.Auth.Commands.RevokeUserSessions;

public sealed class RevokeUserSessionsCommandValidator : AbstractValidator<RevokeUserSessionsCommand>
{
    public RevokeUserSessionsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Идентификатор пользователя обязателен.");
    }
}