using FluentValidation;

namespace JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaireResponses;

public sealed class GetQuestionnaireResponsesQueryValidator : AbstractValidator<GetQuestionnaireResponsesQuery>
{
    public GetQuestionnaireResponsesQueryValidator()
    {
        RuleFor(x => x.QuestionnaireId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}