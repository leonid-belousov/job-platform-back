using FluentValidation;

namespace JobPlatform.BLL.CQRS.CRM.Commands.CompleteCrmTask;

public sealed class CompleteCrmTaskCommandValidator : AbstractValidator<CompleteCrmTaskCommand>
{
    public CompleteCrmTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
