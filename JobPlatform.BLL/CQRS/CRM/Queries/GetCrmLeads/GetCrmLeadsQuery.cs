using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Queries.GetCrmLeads;

public sealed record GetCrmLeadsQuery(
    string? Type,
    string? Status,
    Guid? ResponsibleUserId,
    string? Search,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CrmLeadDto>>
{
    public sealed class GetCrmLeadsQueryHandler : IRequestHandler<GetCrmLeadsQuery, PagedResult<CrmLeadDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetCrmLeadsQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<PagedResult<CrmLeadDto>> Handle(GetCrmLeadsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Set<CrmLead>().AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Type)) query = query.Where(x => x.Type == request.Type);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.ResponsibleUserId.HasValue)
                query = query.Where(x => x.ResponsibleUserId == request.ResponsibleUserId.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new CrmLeadDto(x.Id, x.Type, x.Name, x.Status, x.Source, x.Description,
                    x.ResponsibleUserId, x.CandidateProfileId, x.CompanyId, x.VacancyId, x.ApplicationId, x.CreatedAt,
                    x.UpdatedAt))
                .ToArrayAsync(cancellationToken);
            return new PagedResult<CrmLeadDto>(items, total, request.Page, request.PageSize);
        }
    }
}