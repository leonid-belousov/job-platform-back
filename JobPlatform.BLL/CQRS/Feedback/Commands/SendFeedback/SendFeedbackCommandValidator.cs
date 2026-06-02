using FluentValidation;

namespace JobPlatform.BLL.CQRS.Feedback.Commands.SendFeedback;

public sealed class SendFeedbackCommandValidator : AbstractValidator<SendFeedbackCommand>
{
    public SendFeedbackCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Company).MaximumLength(160);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4_000);
    }
}
