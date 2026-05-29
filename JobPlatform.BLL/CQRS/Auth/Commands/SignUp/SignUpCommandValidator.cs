using FluentValidation;

namespace JobPlatform.BLL.CQRS.Auth.Commands.SignUp;

public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    private static readonly string[] AllowedRoles = { "candidate", "employer", "recruiter" };
    
    public SignUpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Email имеет некорректный формат.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов.")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву.")
            .Matches("[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву.")
            .Matches("[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру.");

        RuleFor(x => x.RoleCode)
            .NotEmpty().WithMessage("Роль обязательна.")
            .Must(x => AllowedRoles.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Регистрация разрешена только для ролей candidate, employer или recruiter.");
    }
}