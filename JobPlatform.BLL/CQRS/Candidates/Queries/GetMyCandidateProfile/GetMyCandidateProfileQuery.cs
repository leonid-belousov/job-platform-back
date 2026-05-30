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
                              .Include(x => x.Languages.Where(language => !language.IsDeleted))
                              .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Профиль кандидата не найден.");

            return new CandidateProfileDto(
                profile.Id,
                profile.FirstName,
                profile.MiddleName,
                profile.LastName,
                profile.DateOfBirth,
                profile.Citizenship,
                profile.CountryOfResidence,
                profile.City,
                profile.Phone,
                profile.DesiredPosition,
                profile.ExpectedSalary,
                profile.Currency,
                profile.About,
                profile.IsVisible,
                profile.JobSearchStatus,
                profile.ModerationStatus,
                profile.ModerationComment,
                profile.ModeratedByUserId,
                profile.ModeratedAt,
                profile.Languages
                    .OrderBy(x => x.LanguageCode)
                    .Select(x => new CandidateLanguageDto(x.Id, x.LanguageCode, x.Level))
                    .ToArray());
        }
    }
}