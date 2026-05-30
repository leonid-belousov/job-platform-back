using FluentValidation;

namespace JobPlatform.BLL.CQRS.Auth.Commands.SignOut;

public class SignOutCommandValidator : AbstractValidator<SignOutCommand>
{
    public SignOutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token обязателен.")
            .MaximumLength(512);
    }
}