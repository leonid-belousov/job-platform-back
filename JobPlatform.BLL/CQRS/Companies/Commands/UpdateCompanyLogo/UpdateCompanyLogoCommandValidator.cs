using FluentValidation;

namespace JobPlatform.BLL.CQRS.Companies.Commands.UpdateCompanyLogo;

public sealed class UpdateCompanyLogoCommandValidator : AbstractValidator<UpdateCompanyLogoCommand>
{
    public UpdateCompanyLogoCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.LogoFileId).NotEmpty();
    }
}
