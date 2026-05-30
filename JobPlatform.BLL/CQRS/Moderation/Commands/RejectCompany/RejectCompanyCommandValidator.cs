using FluentValidation;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.RejectCompany;

public sealed class RejectCompanyCommandValidator : AbstractValidator<RejectCompanyCommand>
{
    public RejectCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}