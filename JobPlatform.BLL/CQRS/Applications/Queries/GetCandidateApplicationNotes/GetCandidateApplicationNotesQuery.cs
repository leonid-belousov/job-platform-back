using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.GetCandidateApplicationNotes;

public sealed record GetCandidateApplicationNotesQuery(Guid CandidateProfileId) : IRequest<IReadOnlyCollection<ApplicationNoteDto>>
{
    public sealed class Handler : IRequestHandler<GetCandidateApplicationNotesQuery, IReadOnlyCollection<ApplicationNoteDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<ApplicationNoteDto>> Handle(GetCandidateApplicationNotesQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var accessibleCompanyIds = _db.Set<CompanyMember>()
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Status == "Active" && !x.IsDeleted)
                .Select(x => x.CompanyId);

            return await _db.Set<ApplicationNote>()
                .AsNoTracking()
                .Include(x => x.AuthorUser)
                .Include(x => x.JobApplication)
                .ThenInclude(x => x.Vacancy)
                .Where(x => !x.IsDeleted
                            && x.JobApplication.CandidateProfileId == request.CandidateProfileId
                            && accessibleCompanyIds.Contains(x.JobApplication.Vacancy.CompanyId))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ApplicationNoteDto(
                    x.Id,
                    x.JobApplicationId,
                    x.JobApplication.CandidateProfileId,
                    x.JobApplication.VacancyId,
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
