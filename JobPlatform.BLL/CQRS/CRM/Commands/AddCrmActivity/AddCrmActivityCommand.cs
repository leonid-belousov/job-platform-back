using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Commands.AddCrmActivity;

public sealed record AddCrmActivityCommand(
    Guid LeadId,
    string Type,
    string Description,
    Guid? RelatedEntityId,
    string? RelatedEntityType) : IRequest<CrmActivityDto>
{
    public class AddCrmActivityCommandHandler : IRequestHandler<AddCrmActivityCommand, CrmActivityDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public AddCrmActivityCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CrmActivityDto> Handle(AddCrmActivityCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            if (!await _db.Set<CrmLead>().AnyAsync(x => x.Id == request.LeadId, cancellationToken))
                throw new KeyNotFoundException("CRM-лид не найден.");

            var activity = new CrmActivity
            {
                LeadId = request.LeadId,
                Type = request.Type,
                Description = request.Description.Trim(),
                CreatedByUserId = userId,
                RelatedEntityId = request.RelatedEntityId,
                RelatedEntityType = request.RelatedEntityType
            };
            await _db.Set<CrmActivity>().AddAsync(activity, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.CrmActivityAdded, nameof(CrmActivity), activity.Id,
                    NewValue: new { activity.LeadId, activity.Type }, UserId: userId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return activity.ToDto();
        }
    }
}