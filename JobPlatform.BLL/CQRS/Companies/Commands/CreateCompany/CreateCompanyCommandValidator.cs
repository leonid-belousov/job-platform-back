using FluentValidation;

namespace JobPlatform.BLL.CQRS.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Industry).MaximumLength(150);
        RuleFor(x => x.Website).MaximumLength(500);
    }
}