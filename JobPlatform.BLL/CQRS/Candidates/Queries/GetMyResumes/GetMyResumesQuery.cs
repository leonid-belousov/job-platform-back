using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Queries.GetMyResumes;

public sealed record GetMyResumesQuery : IRequest<IReadOnlyCollection<ResumeDto>>
{
    public class GetMyResumesQueryHandler : IRequestHandler<GetMyResumesQuery, IReadOnlyCollection<ResumeDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetMyResumesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<ResumeDto>> Handle(GetMyResumesQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var profile = await _db.Set<CandidateProfile>().AsNoTracking()
                              .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Профиль кандидата не найден.");

            return await _db.Set<Resume>()
                .AsNoTracking()
                .Where(x => x.CandidateProfileId == profile.Id && !x.IsDeleted)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x =>
                    new ResumeDto(x.Id, x.CandidateProfileId, x.Title, x.FileId, x.Status, x.IsDefault, x.CreatedAt))
                .ToArrayAsync(cancellationToken);
        }
    }
}