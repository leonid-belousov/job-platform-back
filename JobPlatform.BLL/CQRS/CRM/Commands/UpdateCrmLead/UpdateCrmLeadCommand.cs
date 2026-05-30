using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Commands.UpdateCrmLead;

public sealed record UpdateCrmLeadCommand(
    Guid LeadId,
    string Name,
    string Status,
    string? Source,
    string? Description,
    Guid? ResponsibleUserId) : IRequest<CrmLeadDto>
{
    public class UpdateCrmLeadCommandHandler : IRequestHandler<UpdateCrmLeadCommand, CrmLeadDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public UpdateCrmLeadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CrmLeadDto> Handle(UpdateCrmLeadCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var lead = await _db.Set<CrmLead>().FirstOrDefaultAsync(x => x.Id == request.LeadId, cancellationToken)
                       ?? throw new KeyNotFoundException("CRM-лид не найден.");
            if (request.ResponsibleUserId.HasValue && !await _db.Set<User>()
                    .AnyAsync(x => x.Id == request.ResponsibleUserId.Value, cancellationToken))
                throw new KeyNotFoundException("Ответственный пользователь не найден.");

            var oldValue = new { lead.Name, lead.Status, lead.Source, lead.Description, lead.ResponsibleUserId };
            var oldStatus = lead.Status;
            lead.Name = request.Name.Trim();
            lead.Status = request.Status;
            lead.Source = request.Source;
            lead.Description = request.Description;
            lead.ResponsibleUserId = request.ResponsibleUserId;

            if (!string.Equals(oldStatus, lead.Status, StringComparison.OrdinalIgnoreCase))
            {
                await _db.Set<CrmActivity>().AddAsync(new CrmActivity
                {
                    LeadId = lead.Id,
                    Type = CrmActivityTypes.StatusChanged,
                    Description = $"Статус изменен: {oldStatus} -> {lead.Status}.",
                    CreatedByUserId = userId
                }, cancellationToken);
            }

            await _auditService.AddAsync(
                new AuditEvent(AuditActions.CrmLeadUpdated, nameof(CrmLead), lead.Id, OldValue: oldValue,
                    NewValue: new { lead.Name, lead.Status, lead.Source, lead.Description, lead.ResponsibleUserId },
                    UserId: userId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return lead.ToDto();
        }
    }
}