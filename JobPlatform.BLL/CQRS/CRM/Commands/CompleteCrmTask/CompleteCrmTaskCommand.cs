using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Commands.CompleteCrmTask;

public sealed record CompleteCrmTaskCommand(Guid TaskId) : IRequest<CrmTaskDto>
{
    public class CompleteCrmTaskCommandHandler : IRequestHandler<CompleteCrmTaskCommand, CrmTaskDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public CompleteCrmTaskCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CrmTaskDto> Handle(CompleteCrmTaskCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var task = await _db.Set<CrmTask>().FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken)
                       ?? throw new KeyNotFoundException("CRM-задача не найдена.");
            if (task.Status != CrmTaskStatuses.Completed)
            {
                task.Status = CrmTaskStatuses.Completed;
                task.CompletedAt = DateTimeOffset.UtcNow;
                task.CompletedByUserId = userId;
                await _db.Set<CrmActivity>().AddAsync(new CrmActivity
                {
                    LeadId = task.LeadId, Type = CrmActivityTypes.TaskCompleted,
                    Description = $"Задача выполнена: {task.Title}.", CreatedByUserId = userId,
                    RelatedEntityId = task.Id, RelatedEntityType = nameof(CrmTask)
                }, cancellationToken);
                await _auditService.AddAsync(
                    new AuditEvent(AuditActions.CrmTaskCompleted, nameof(CrmTask), task.Id,
                        NewValue: new { task.Status, task.CompletedAt, task.CompletedByUserId }, UserId: userId),
                    cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return task.ToDto();
        }
    }
}