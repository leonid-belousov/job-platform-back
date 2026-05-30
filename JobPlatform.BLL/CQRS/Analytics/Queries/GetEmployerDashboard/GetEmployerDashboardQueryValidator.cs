using FluentValidation;

namespace JobPlatform.BLL.CQRS.Analytics.Queries.GetEmployerDashboard;

public sealed class GetEmployerDashboardQueryValidator : AbstractValidator<GetEmployerDashboardQuery>
{
    public GetEmployerDashboardQueryValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}
