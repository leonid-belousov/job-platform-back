using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.UpdateApplicationNote;

public sealed record UpdateApplicationNoteCommand(Guid NoteId, string Text) : IRequest<ApplicationNoteDto>
{
    public sealed class Handler : IRequestHandler<UpdateApplicationNoteCommand, ApplicationNoteDto>
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

        public async Task<ApplicationNoteDto> Handle(UpdateApplicationNoteCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var note = await _db.Set<ApplicationNote>()
                           .Include(x => x.JobApplication)
                           .ThenInclude(x => x.Vacancy)
                           .Include(x => x.AuthorUser)
                           .FirstOrDefaultAsync(x => x.Id == request.NoteId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("Заметка не найдена.");

            var hasAccess = await _db.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == note.JobApplication.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active" && !x.IsDeleted,
                cancellationToken);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Нет доступа к заметке.");
            }

            var oldText = note.Text;
            note.Text = request.Text.Trim();
            note.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.ApplicationNoteUpdated,
                EntityType: nameof(ApplicationNote),
                EntityId: note.Id,
                OldValue: new { Text = oldText },
                NewValue: new { note.JobApplicationId, note.Text },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            var authorName = string.Join(" ", new[] { note.AuthorUser.FirstName, note.AuthorUser.LastName }
                .Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

            return new ApplicationNoteDto(
                note.Id,
                note.JobApplicationId,
                note.JobApplication.CandidateProfileId,
                note.JobApplication.VacancyId,
                note.AuthorUserId,
                string.IsNullOrWhiteSpace(authorName) ? note.AuthorUser.Email : authorName,
                note.Text,
                note.CreatedAt,
                note.UpdatedAt);
        }
    }
}
