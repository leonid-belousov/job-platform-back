using FluentValidation;
using JobPlatform.Core.Entities.CRM;

namespace JobPlatform.BLL.CQRS.CRM.Commands.AddCrmActivity;

public sealed class AddCrmActivityCommandValidator : AbstractValidator<AddCrmActivityCommand>
{
    public AddCrmActivityCommandValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().Must(x => CrmActivityTypes.All.Contains(x)).WithMessage("Недопустимый тип CRM-активности.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.RelatedEntityType).MaximumLength(100);
    }
}
