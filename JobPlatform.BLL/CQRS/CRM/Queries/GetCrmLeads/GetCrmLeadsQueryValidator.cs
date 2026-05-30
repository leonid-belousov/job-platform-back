using FluentValidation;
using JobPlatform.Core.Entities.CRM;

namespace JobPlatform.BLL.CQRS.CRM.Queries.GetCrmLeads;

public sealed class GetCrmLeadsQueryValidator : AbstractValidator<GetCrmLeadsQuery>
{
    public GetCrmLeadsQueryValidator()
    {
        RuleFor(x => x.Type).Must(x => string.IsNullOrWhiteSpace(x) || CrmLeadTypes.All.Contains(x)).WithMessage("Недопустимый тип CRM-лида.");
        RuleFor(x => x.Status).Must(x => string.IsNullOrWhiteSpace(x) || CrmLeadStatuses.All.Contains(x)).WithMessage("Недопустимый статус CRM-лида.");
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}