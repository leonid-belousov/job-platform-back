using FluentValidation;
using JobPlatform.Core.Entities.CRM;

namespace JobPlatform.BLL.CQRS.CRM.Commands.UpdateCrmLead;

public sealed class UpdateCrmLeadCommandValidator : AbstractValidator<UpdateCrmLeadCommand>
{
    public UpdateCrmLeadCommandValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Status).NotEmpty().Must(x => CrmLeadStatuses.All.Contains(x))
            .WithMessage("Недопустимый статус CRM-лида.");
        RuleFor(x => x.Source).MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}