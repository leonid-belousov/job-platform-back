using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.CreateApplicationNote;

public sealed record CreateApplicationNoteCommand(Guid ApplicationId, string Text) : IRequest<ApplicationNoteDto>
{
    public sealed class Handler : IRequestHandler<CreateApplicationNoteCommand, ApplicationNoteDto>
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

        public async Task<ApplicationNoteDto> Handle(CreateApplicationNoteCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var application = await _db.Set<JobApplication>()
                                  .Include(x => x.Vacancy)
                                  .FirstOrDefaultAsync(x => x.Id == request.ApplicationId && !x.IsDeleted,
                                      cancellationToken)
                              ?? throw new InvalidOperationException("Отклик не найден.");

            var hasAccess = await _db.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == application.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active" && !x.IsDeleted,
                cancellationToken);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Нет доступа к заметкам этого отклика.");
            }

            var note = new ApplicationNote
            {
                JobApplicationId = application.Id,
                AuthorUserId = userId,
                Text = request.Text.Trim()
            };

            await _db.Set<ApplicationNote>().AddAsync(note, cancellationToken);
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.ApplicationNoteCreated,
                EntityType: nameof(ApplicationNote),
                EntityId: note.Id,
                NewValue: new { note.JobApplicationId, application.CandidateProfileId, application.VacancyId, note.Text },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return new ApplicationNoteDto(
                note.Id,
                note.JobApplicationId,
                application.CandidateProfileId,
                application.VacancyId,
                note.AuthorUserId,
                _currentUser.Email ?? string.Empty,
                note.Text,
                note.CreatedAt,
                note.UpdatedAt);
        }
    }
}