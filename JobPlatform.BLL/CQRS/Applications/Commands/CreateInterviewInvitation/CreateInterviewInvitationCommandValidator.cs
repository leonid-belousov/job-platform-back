using FluentValidation;
using JobPlatform.Core.Entities.Applications;

namespace JobPlatform.BLL.CQRS.Applications.Commands.CreateInterviewInvitation;

public sealed class CreateInterviewInvitationCommandValidator : AbstractValidator<CreateInterviewInvitationCommand>
{
    public CreateInterviewInvitationCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTimeOffset.UtcNow);
        RuleFor(x => x.Format)
            .NotEmpty()
            .Must(x => InterviewInvitationFormats.All.Contains(x, StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.MeetingUrl).MaximumLength(1000);
        RuleFor(x => x.Message).MaximumLength(2000);
    }
}