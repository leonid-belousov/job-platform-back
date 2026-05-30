using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Queries.GetCrmTimeline;

public sealed record GetCrmTimelineQuery(Guid LeadId) : IRequest<IReadOnlyCollection<CrmTimelineItemDto>>
{
    public sealed class
        GetCrmTimelineQueryHandler : IRequestHandler<GetCrmTimelineQuery, IReadOnlyCollection<CrmTimelineItemDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetCrmTimelineQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyCollection<CrmTimelineItemDto>> Handle(GetCrmTimelineQuery request,
            CancellationToken cancellationToken)
        {
            if (!await _db.Set<CrmLead>().AnyAsync(x => x.Id == request.LeadId, cancellationToken))
                throw new KeyNotFoundException("CRM-лид не найден.");

            var activities = await _db.Set<CrmActivity>().AsNoTracking()
                .Where(x => x.LeadId == request.LeadId)
                .Select(x => new CrmTimelineItemDto(x.Id, "activity", x.Type, x.Type, x.Description, null,
                    x.CreatedByUserId, x.CreatedAt, null))
                .ToArrayAsync(cancellationToken);

            var tasks = await _db.Set<CrmTask>().AsNoTracking()
                .Where(x => x.LeadId == request.LeadId)
                .Select(x => new CrmTimelineItemDto(x.Id, "task", "task", x.Title, x.Description, x.Status,
                    x.ResponsibleUserId, x.CreatedAt, x.DueDate))
                .ToArrayAsync(cancellationToken);

            return activities.Concat(tasks).OrderByDescending(x => x.CreatedAt).ToArray();
        }
    }
}