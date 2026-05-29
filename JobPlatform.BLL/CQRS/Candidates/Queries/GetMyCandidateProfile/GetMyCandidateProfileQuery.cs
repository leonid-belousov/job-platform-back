using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Queries.GetMyCandidateProfile;

public sealed record GetMyCandidateProfileQuery : IRequest<CandidateProfileDto>
{
    public class GetMyCandidateProfileQueryHandler : IRequestHandler<GetMyCandidateProfileQuery, CandidateProfileDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetMyCandidateProfileQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<CandidateProfileDto> Handle(GetMyCandidateProfileQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();


            var profile = await _db.Set<CandidateProfile>().AsNoTracking()
                              .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Профиль кандидата не найден.");


            return new CandidateProfileDto(profile.Id, profile.FirstName, profile.LastName, profile.City,
                profile.DesiredPosition, profile.ExpectedSalary);
        }
    }
}