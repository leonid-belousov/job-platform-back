using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaires;

public sealed record GetQuestionnairesQuery(
    string? EntityType,
    Guid? EntityId,
    bool? IsActive,
    string? Search,
    int Page,
    int PageSize) : IRequest<PagedResult<QuestionnaireListItemDto>>
{
    public sealed class
        GetQuestionnairesQueryHandler : IRequestHandler<GetQuestionnairesQuery, PagedResult<QuestionnaireListItemDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetQuestionnairesQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<PagedResult<QuestionnaireListItemDto>> Handle(GetQuestionnairesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _db.Set<Questionnaire>().AsNoTracking().Include(x => x.Sections).Include(x => x.Responses)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.EntityType))
                query = query.Where(x => x.EntityType == request.EntityType);
            if (request.EntityId.HasValue) query = query.Where(x => x.EntityId == request.EntityId.Value);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Title.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            var total = await query.CountAsync(cancellationToken);
            var entities = await query.OrderByDescending(x => x.CreatedAt).Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize).ToArrayAsync(cancellationToken);
            var items = entities.Select(x => x.ToListItemDto()).ToArray();
            return new PagedResult<QuestionnaireListItemDto>(items, total, request.Page, request.PageSize);
        }
    }
}