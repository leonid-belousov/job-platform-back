using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Commands.CreateCrmTask;

public sealed record CreateCrmTaskCommand(
    Guid LeadId,
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    Guid? ResponsibleUserId) : IRequest<CrmTaskDto>
{
    public class CreateCrmTaskCommandHandler : IRequestHandler<CreateCrmTaskCommand, CrmTaskDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public CreateCrmTaskCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CrmTaskDto> Handle(CreateCrmTaskCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            if (!await _db.Set<CrmLead>().AnyAsync(x => x.Id == request.LeadId, cancellationToken))
                throw new KeyNotFoundException("CRM-лид не найден.");
            if (request.ResponsibleUserId.HasValue && !await _db.Set<User>()
                    .AnyAsync(x => x.Id == request.ResponsibleUserId.Value, cancellationToken))
                throw new KeyNotFoundException("Ответственный пользователь не найден.");

            var task = new CrmTask
            {
                LeadId = request.LeadId,
                Title = request.Title.Trim(),
                Description = request.Description,
                DueDate = request.DueDate,
                ResponsibleUserId = request.ResponsibleUserId,
                CreatedByUserId = userId
            };
            await _db.Set<CrmTask>().AddAsync(task, cancellationToken);
            await _db.Set<CrmActivity>().AddAsync(new CrmActivity
            {
                LeadId = request.LeadId, Type = CrmActivityTypes.TaskCreated,
                Description = $"Создана задача: {task.Title}.", CreatedByUserId = userId, RelatedEntityId = task.Id,
                RelatedEntityType = nameof(CrmTask)
            }, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.CrmTaskCreated, nameof(CrmTask), task.Id,
                    NewValue: new { task.LeadId, task.Title, task.DueDate, task.ResponsibleUserId }, UserId: userId),
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return task.ToDto();
        }
    }
}