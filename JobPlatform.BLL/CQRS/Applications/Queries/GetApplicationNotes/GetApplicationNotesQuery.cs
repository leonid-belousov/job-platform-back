using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.GetApplicationNotes;

public sealed record GetApplicationNotesQuery(Guid ApplicationId) : IRequest<IReadOnlyCollection<ApplicationNoteDto>>
{
    public sealed class Handler : IRequestHandler<GetApplicationNotesQuery, IReadOnlyCollection<ApplicationNoteDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<ApplicationNoteDto>> Handle(GetApplicationNotesQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var application = await _db.Set<JobApplication>()
                                  .AsNoTracking()
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

            return await _db.Set<ApplicationNote>()
                .AsNoTracking()
                .Include(x => x.AuthorUser)
                .Where(x => x.JobApplicationId == request.ApplicationId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ApplicationNoteDto(
                    x.Id,
                    x.JobApplicationId,
                    application.CandidateProfileId,
                    application.VacancyId,
                    x.AuthorUserId,
                    string.IsNullOrWhiteSpace((x.AuthorUser.FirstName + " " + x.AuthorUser.LastName).Trim())
                        ? x.AuthorUser.Email
                        : (x.AuthorUser.FirstName + " " + x.AuthorUser.LastName).Trim(),
                    x.Text,
                    x.CreatedAt,
                    x.UpdatedAt))
                .ToArrayAsync(cancellationToken);
        }
    }
}
