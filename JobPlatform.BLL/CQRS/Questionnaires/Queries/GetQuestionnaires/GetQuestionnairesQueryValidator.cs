using FluentValidation;
using JobPlatform.Core.Entities.Questionnaires;

namespace JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaires;

public sealed class GetQuestionnairesQueryValidator : AbstractValidator<GetQuestionnairesQuery>
{
    public GetQuestionnairesQueryValidator()
    {
        RuleFor(x => x.EntityType).Must(x => x is null || QuestionnaireEntityTypes.All.Contains(x))
            .WithMessage("Недопустимый тип сущности.");
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}