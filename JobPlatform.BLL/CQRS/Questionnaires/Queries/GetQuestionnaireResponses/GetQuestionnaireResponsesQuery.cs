using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaireResponses;

public sealed record GetQuestionnaireResponsesQuery(Guid QuestionnaireId, int Page, int PageSize)
    : IRequest<PagedResult<QuestionnaireResponseDto>>
{
    public sealed class GetQuestionnaireResponsesQueryHandler : IRequestHandler<GetQuestionnaireResponsesQuery,
        PagedResult<QuestionnaireResponseDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetQuestionnaireResponsesQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<PagedResult<QuestionnaireResponseDto>> Handle(GetQuestionnaireResponsesQuery request,
            CancellationToken cancellationToken)
        {
            if (!await _db.Set<Questionnaire>().AnyAsync(x => x.Id == request.QuestionnaireId, cancellationToken))
                throw new KeyNotFoundException("Анкета не найдена.");
            var query = _db.Set<QuestionnaireResponse>().AsNoTracking()
                .Where(x => x.QuestionnaireId == request.QuestionnaireId).Include(x => x.Answers)
                .ThenInclude(x => x.Question);
            var total = await query.CountAsync(cancellationToken);
            var entities = await query.OrderByDescending(x => x.SubmittedAt).Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize).ToArrayAsync(cancellationToken);
            var items = entities.Select(x => x.ToDto()).ToArray();
            return new PagedResult<QuestionnaireResponseDto>(items, total, request.Page, request.PageSize);
        }
    }
}