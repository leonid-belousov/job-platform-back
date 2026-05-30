using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Audit.DTO;
using JobPlatform.Core.Entities.Audit;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Audit.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    string? Action,
    string? EntityType,
    Guid? EntityId,
    Guid? UserId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<AuditLogDto>>
{
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetAuditLogsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request,
            CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

            var query = _db.Set<AuditLog>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                var action = request.Action.Trim();
                query = query.Where(x => x.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(request.EntityType))
            {
                var entityType = request.EntityType.Trim();
                query = query.Where(x => x.EntityType == entityType);
            }

            if (request.EntityId.HasValue)
                query = query.Where(x => x.EntityId == request.EntityId.Value);

            if (request.UserId.HasValue)
                query = query.Where(x => x.UserId == request.UserId.Value);

            if (request.From.HasValue)
                query = query.Where(x => x.CreatedAt >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(x => x.CreatedAt <= request.To.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AuditLogDto(
                    x.Id,
                    x.UserId,
                    x.Action,
                    x.EntityType,
                    x.EntityId,
                    x.OldValue,
                    x.NewValue,
                    x.IpAddress,
                    x.UserAgent,
                    x.CreatedAt))
                .ToListAsync(cancellationToken);

            return new PagedResult<AuditLogDto>(items, total, page, pageSize);
        }
    }
}