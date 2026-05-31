using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.DeleteApplicationNote;

public sealed record DeleteApplicationNoteCommand(Guid NoteId) : IRequest
{
    public sealed class Handler : IRequestHandler<DeleteApplicationNoteCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task Handle(DeleteApplicationNoteCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var note = await _db.Set<ApplicationNote>()
                           .Include(x => x.JobApplication)
                           .ThenInclude(x => x.Vacancy)
                           .FirstOrDefaultAsync(x => x.Id == request.NoteId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("Заметка не найдена.");

            var hasAccess = await _db.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == note.JobApplication.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active" && !x.IsDeleted,
                cancellationToken);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Нет доступа к заметке.");
            }

            note.IsDeleted = true;
            note.DeletedAt = DateTimeOffset.UtcNow;
            note.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.ApplicationNoteDeleted,
                EntityType: nameof(ApplicationNote),
                EntityId: note.Id,
                OldValue: new { note.JobApplicationId, note.Text },
                NewValue: new { note.IsDeleted, note.DeletedAt },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
