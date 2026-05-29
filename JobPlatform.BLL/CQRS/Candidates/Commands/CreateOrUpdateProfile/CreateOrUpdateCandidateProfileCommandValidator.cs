using FluentValidation;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateOrUpdateProfile;

public class CreateOrUpdateCandidateProfileCommandValidator : AbstractValidator<CreateOrUpdateCandidateProfileCommand>
{
    public CreateOrUpdateCandidateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).MaximumLength(120);
        RuleFor(x => x.DesiredPosition).MaximumLength(200);
        RuleFor(x => x.ExpectedSalary).GreaterThanOrEqualTo(0).When(x => x.ExpectedSalary.HasValue);
        RuleFor(x => x.About).MaximumLength(4000);
    }
}