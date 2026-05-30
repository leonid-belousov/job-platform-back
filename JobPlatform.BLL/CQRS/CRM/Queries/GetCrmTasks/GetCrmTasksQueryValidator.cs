using FluentValidation;
using JobPlatform.Core.Entities.CRM;

namespace JobPlatform.BLL.CQRS.CRM.Queries.GetCrmTasks;

public sealed class GetCrmTasksQueryValidator : AbstractValidator<GetCrmTasksQuery>
{
    public GetCrmTasksQueryValidator()
    {
        RuleFor(x => x.Status).Must(x => string.IsNullOrWhiteSpace(x) || CrmTaskStatuses.All.Contains(x)).WithMessage("Недопустимый статус CRM-задачи.");
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
