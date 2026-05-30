using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Queries.GetCrmTasks;

public sealed record GetCrmTasksQuery(
    Guid? LeadId,
    string? Status,
    Guid? ResponsibleUserId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CrmTaskDto>>
{
    public sealed class GetCrmTasksQueryHandler : IRequestHandler<GetCrmTasksQuery, PagedResult<CrmTaskDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetCrmTasksQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<PagedResult<CrmTaskDto>> Handle(GetCrmTasksQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Set<CrmTask>().AsNoTracking().AsQueryable();
            if (request.LeadId.HasValue) query = query.Where(x => x.LeadId == request.LeadId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.ResponsibleUserId.HasValue)
                query = query.Where(x => x.ResponsibleUserId == request.ResponsibleUserId.Value);
            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderBy(x => x.DueDate ?? DateTimeOffset.MaxValue)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new CrmTaskDto(x.Id, x.LeadId, x.Title, x.Description, x.DueDate, x.Status,
                    x.ResponsibleUserId, x.CreatedByUserId, x.CompletedAt, x.CompletedByUserId, x.CreatedAt))
                .ToArrayAsync(cancellationToken);
            return new PagedResult<CrmTaskDto>(items, total, request.Page, request.PageSize);
        }
    }
}