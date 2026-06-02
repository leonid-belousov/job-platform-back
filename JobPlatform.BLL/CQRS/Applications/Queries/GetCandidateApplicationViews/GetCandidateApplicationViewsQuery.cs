using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.GetCandidateApplicationViews;

public sealed record GetCandidateApplicationViewsQuery(string? Status) : IRequest<IReadOnlyCollection<CandidateApplicationListItemDto>>
{
    public sealed class Handler : IRequestHandler<GetCandidateApplicationViewsQuery, IReadOnlyCollection<CandidateApplicationListItemDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<CandidateApplicationListItemDto>> Handle(GetCandidateApplicationViewsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var normalizedStatus = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToLowerInvariant();

            var candidate = await _db.Set<CandidateProfile>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Профиль кандидата не найден.");

            var query = _db.Set<JobApplication>()
                .AsNoTracking()
                .Where(x => x.CandidateProfileId == candidate.Id && !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(normalizedStatus))
            {
                query = query.Where(x => x.Status == normalizedStatus);
            }

            return await query
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .Select(x => new CandidateApplicationListItemDto(
                    x.Id,
                    x.VacancyId,
                    x.Vacancy.Title,
                    x.Vacancy.Company.Name,
                    null,
                    x.Status,
                    x.CoverLetter,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.Vacancy.City,
                    x.Vacancy.Country,
                    x.Vacancy.EmploymentType,
                    x.Vacancy.WorkFormat,
                    x.Vacancy.ExperienceLevel,
                    x.Vacancy.SalaryFrom,
                    x.Vacancy.SalaryTo,
                    x.Vacancy.Currency,
                    x.Vacancy.PublishedAt,
                    x.InterviewInvitations
                        .Where(invitation => !invitation.IsDeleted)
                        .OrderByDescending(invitation => invitation.ScheduledAt)
                        .Select(invitation => (DateTimeOffset?)invitation.ScheduledAt)
                        .FirstOrDefault(),
                    x.InterviewInvitations
                        .Where(invitation => !invitation.IsDeleted)
                        .OrderByDescending(invitation => invitation.ScheduledAt)
                        .Select(invitation => invitation.Status)
                        .FirstOrDefault(),
                    false))
                .ToArrayAsync(cancellationToken);
        }
    }
}
