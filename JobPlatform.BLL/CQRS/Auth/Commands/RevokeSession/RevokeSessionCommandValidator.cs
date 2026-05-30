using FluentValidation;

namespace JobPlatform.BLL.CQRS.Auth.Commands.RevokeSession;

public sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("Идентификатор сессии обязателен.");
    }
}
