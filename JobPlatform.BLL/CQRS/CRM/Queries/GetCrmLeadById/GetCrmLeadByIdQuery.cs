using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Queries.GetCrmLeadById;

public sealed record GetCrmLeadByIdQuery(Guid LeadId) : IRequest<CrmLeadDto>
{
    public class GetCrmLeadByIdQueryHandler : IRequestHandler<GetCrmLeadByIdQuery, CrmLeadDto>
    {
        private readonly IApplicationDbContext _db;

        public GetCrmLeadByIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CrmLeadDto> Handle(GetCrmLeadByIdQuery request, CancellationToken cancellationToken)
        {
            var lead = await _db.Set<CrmLead>().AsNoTracking()
                           .FirstOrDefaultAsync(x => x.Id == request.LeadId, cancellationToken)
                       ?? throw new KeyNotFoundException("CRM-лид не найден.");
            return lead.ToDto();
        }
    }
}