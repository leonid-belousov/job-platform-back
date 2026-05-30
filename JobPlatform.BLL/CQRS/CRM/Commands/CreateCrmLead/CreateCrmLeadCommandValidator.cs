using FluentValidation;
using JobPlatform.Core.Entities.CRM;

namespace JobPlatform.BLL.CQRS.CRM.Commands.CreateCrmLead;

public sealed class CreateCrmLeadCommandValidator : AbstractValidator<CreateCrmLeadCommand>
{
    public CreateCrmLeadCommandValidator()
    {
        RuleFor(x => x.Type).NotEmpty().Must(x => CrmLeadTypes.All.Contains(x)).WithMessage("Недопустимый тип CRM-лида.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Source).MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}
