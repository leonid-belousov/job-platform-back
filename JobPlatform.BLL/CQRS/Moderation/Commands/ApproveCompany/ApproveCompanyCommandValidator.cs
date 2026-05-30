using FluentValidation;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.ApproveCompany;

public sealed class ApproveCompanyCommandValidator : AbstractValidator<ApproveCompanyCommand>
{
    public ApproveCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}