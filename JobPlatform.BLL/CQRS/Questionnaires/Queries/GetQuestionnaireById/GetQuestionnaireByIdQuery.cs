using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaireById;

public sealed record GetQuestionnaireByIdQuery(Guid Id) : IRequest<QuestionnaireDto>
{
    public sealed class GetQuestionnaireByIdQueryHandler : IRequestHandler<GetQuestionnaireByIdQuery, QuestionnaireDto>
    {
        private readonly IApplicationDbContext _db;
        public GetQuestionnaireByIdQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<QuestionnaireDto> Handle(GetQuestionnaireByIdQuery request,
            CancellationToken cancellationToken)
        {
            var questionnaire = await _db.Set<Questionnaire>().AsNoTracking()
                                    .Include(x => x.Sections).ThenInclude(x => x.Questions).ThenInclude(x => x.Options)
                                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                                ?? throw new KeyNotFoundException("Анкета не найдена.");
            return questionnaire.ToDto();
        }
    }
}