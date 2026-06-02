using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.GetCandidateApplicationDetails;

public sealed record GetCandidateApplicationDetailsQuery(Guid ApplicationId) : IRequest<CandidateApplicationDetailsDto>
{
    public sealed class Handler : IRequestHandler<GetCandidateApplicationDetailsQuery, CandidateApplicationDetailsDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<CandidateApplicationDetailsDto> Handle(GetCandidateApplicationDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var candidate = await _db.Set<CandidateProfile>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Профиль кандидата не найден.");

            var application = await _db.Set<JobApplication>()
                                  .AsNoTracking()
                                  .Include(x => x.Vacancy)
                                  .ThenInclude(x => x.Company)
                                  .Include(x => x.StatusHistory.Where(history => !history.IsDeleted))
                                  .Include(x => x.InterviewInvitations.Where(invitation => !invitation.IsDeleted))
                                  .FirstOrDefaultAsync(x => x.Id == request.ApplicationId
                                                            && x.CandidateProfileId == candidate.Id
                                                            && !x.IsDeleted,
                                      cancellationToken)
                              ?? throw new KeyNotFoundException("Отклик не найден.");

            return new CandidateApplicationDetailsDto(
                application.Id,
                application.VacancyId,
                application.Vacancy.Title,
                application.Vacancy.Company.Name,
                application.Vacancy.Company.Description,
                application.Vacancy.Company.Industry,
                application.Vacancy.Company.Website,
                null,
                application.Status,
                application.CoverLetter,
                application.ResumeId,
                application.CreatedAt,
                application.UpdatedAt,
                application.Vacancy.City,
                application.Vacancy.Country,
                application.Vacancy.EmploymentType,
                application.Vacancy.WorkFormat,
                application.Vacancy.ExperienceLevel,
                application.Vacancy.SalaryFrom,
                application.Vacancy.SalaryTo,
                application.Vacancy.Currency,
                application.Vacancy.Description,
                application.Vacancy.Requirements,
                application.Vacancy.Responsibilities,
                application.Vacancy.Conditions,
                application.Vacancy.PublishedAt,
                application.StatusHistory
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new CandidateApplicationStatusHistoryDto(
                        x.Id,
                        x.OldStatus ?? string.Empty,
                        x.NewStatus,
                        x.Comment,
                        x.CreatedAt))
                    .ToArray(),
                application.InterviewInvitations
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ScheduledAt)
                    .Select(x => new CandidateApplicationInterviewDto(
                        x.Id,
                        x.ScheduledAt,
                        x.Format,
                        x.Location,
                        x.MeetingUrl,
                        x.Message,
                        x.Status,
                        x.CreatedAt,
                        x.CandidateRespondedAt))
                    .ToArray());
        }
    }
}
