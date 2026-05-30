using JobPlatform.BLL.CQRS.Audit.DTO;
using JobPlatform.Core.Entities.Audit;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Audit.Queries.GetAuditLogById;

public record GetAuditLogByIdQuery(Guid AuditLogId) : IRequest<AuditLogDto>
{
    public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, AuditLogDto>
    {
        private readonly IApplicationDbContext _db;

        public GetAuditLogByIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AuditLogDto> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
        {
            return await _db.Set<AuditLog>()
                       .AsNoTracking()
                       .Where(x => x.Id == request.AuditLogId)
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
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new KeyNotFoundException("Запись аудита не найдена.");
        }
    }
}