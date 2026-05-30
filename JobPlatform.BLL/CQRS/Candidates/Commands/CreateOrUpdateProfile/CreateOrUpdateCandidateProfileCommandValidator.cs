using FluentValidation;
using JobPlatform.Core.Entities.Candidates;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateOrUpdateProfile;

public class CreateOrUpdateCandidateProfileCommandValidator : AbstractValidator<CreateOrUpdateCandidateProfileCommand>
{
    private static readonly string[] AllowedLanguageLevels =
    {
        "beginner",
        "elementary",
        "intermediate",
        "upper_intermediate",
        "advanced",
        "native"
    };

    public CreateOrUpdateCandidateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(x => x <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-14)))
            .WithMessage("Кандидат должен быть не младше 14 лет.");

        RuleFor(x => x.Citizenship).NotEmpty().MaximumLength(80);
        RuleFor(x => x.CountryOfResidence).NotEmpty().MaximumLength(80);
        RuleFor(x => x.City).MaximumLength(120);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.DesiredPosition).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExpectedSalary).GreaterThanOrEqualTo(0).When(x => x.ExpectedSalary.HasValue);
        RuleFor(x => x.Currency).MaximumLength(10);
        RuleFor(x => x.About).MaximumLength(4000);

        RuleFor(x => x.JobSearchStatus)
            .NotEmpty()
            .Must(x => CandidateJobSearchStatuses.All.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Недопустимый статус поиска работы.");

        RuleFor(x => x.Languages)
            .NotNull()
            .Must(x => x.Count > 0)
            .WithMessage("Необходимо указать минимум один язык.");

        RuleForEach(x => x.Languages).ChildRules(language =>
        {
            language.RuleFor(x => x.LanguageCode).NotEmpty().MaximumLength(20);
            language.RuleFor(x => x.Level)
                .NotEmpty()
                .MaximumLength(40)
                .Must(x => AllowedLanguageLevels.Contains(x, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Недопустимый уровень владения языком.");
        });
    }
}