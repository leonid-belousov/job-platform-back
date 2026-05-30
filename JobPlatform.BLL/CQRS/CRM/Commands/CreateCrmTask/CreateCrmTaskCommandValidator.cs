using FluentValidation;

namespace JobPlatform.BLL.CQRS.CRM.Commands.CreateCrmTask;

public sealed class CreateCrmTaskCommandValidator : AbstractValidator<CreateCrmTaskCommand>
{
    public CreateCrmTaskCommandValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}