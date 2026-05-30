using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.GetCandidateApplications;

public sealed record GetCandidateApplicationsQuery : IRequest<IReadOnlyCollection<ApplicationDto>>
{
    public class
        GetCandidateApplicationsQueryHandler : IRequestHandler<GetCandidateApplicationsQuery,
        IReadOnlyCollection<ApplicationDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetCandidateApplicationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<ApplicationDto>> Handle(GetCandidateApplicationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var candidate = await _db.Set<CandidateProfile>().AsNoTracking()
                                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Профиль кандидата не найден.");

            return await _db.Set<JobApplication>()
                .AsNoTracking()
                .Where(x => x.CandidateProfileId == candidate.Id && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ApplicationDto(x.Id, x.VacancyId, x.Vacancy.Title, x.CandidateProfileId,
                    (x.CandidateProfile.FirstName + " " + x.CandidateProfile.LastName).Trim(), null, null, false,
                    x.ResumeId, x.Status, x.CoverLetter, x.CreatedAt))
                .ToArrayAsync(cancellationToken);
        }
    }
}