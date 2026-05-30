using JobPlatform.BLL.CQRS.Analytics.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Analytics.Queries.GetEmployerDashboard;

public sealed record GetEmployerDashboardQuery(Guid CompanyId) : IRequest<EmployerDashboardDto>
{
    public sealed class
        GetEmployerDashboardQueryHandler : IRequestHandler<GetEmployerDashboardQuery, EmployerDashboardDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetEmployerDashboardQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<EmployerDashboardDto> Handle(GetEmployerDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var company = await _db.Set<Company>().AsNoTracking()
                              .FirstOrDefaultAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken)
                          ?? throw new KeyNotFoundException("Компания не найдена.");

            var hasAccess = await _db.Set<CompanyMember>().AsNoTracking().AnyAsync(
                x => x.CompanyId == request.CompanyId && x.UserId == userId && x.Status == "Active", cancellationToken);
            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к аналитике компании.");

            var vacancyIds = await _db.Set<JobVacancy>().AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == request.CompanyId)
                .Select(x => x.Id)
                .ToArrayAsync(cancellationToken);

            var applicationsQuery = _db.Set<JobApplication>().AsNoTracking()
                .Where(x => !x.IsDeleted && vacancyIds.Contains(x.VacancyId));
            var applicationsByStatus = await applicationsQuery
                .GroupBy(x => x.Status)
                .Select(x => new StatusCountDto(x.Key, x.Count()))
                .OrderBy(x => x.Status)
                .ToArrayAsync(cancellationToken);

            var vacancyRows = await _db.Set<JobVacancy>().AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == request.CompanyId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.Title, x.Status, x.ModerationStatus })
                .ToArrayAsync(cancellationToken);

            var applicationStatusRows = await _db.Set<JobApplication>().AsNoTracking()
                .Where(x => !x.IsDeleted && vacancyIds.Contains(x.VacancyId))
                .GroupBy(x => new { x.VacancyId, x.Status })
                .Select(x => new { x.Key.VacancyId, x.Key.Status, Count = x.Count() })
                .ToArrayAsync(cancellationToken);

            var vacancyAnalytics = vacancyRows.Select(v =>
            {
                var statuses = applicationStatusRows
                    .Where(x => x.VacancyId == v.Id)
                    .Select(x => new StatusCountDto(x.Status, x.Count))
                    .OrderBy(x => x.Status)
                    .ToArray();
                return new VacancyAnalyticsDto(v.Id, v.Title, v.Status, v.ModerationStatus, statuses.Sum(x => x.Count),
                    statuses);
            }).ToArray();

            return new EmployerDashboardDto(
                CompanyId: company.Id,
                CompanyName: company.Name,
                VacanciesTotal: vacancyRows.Length,
                VacanciesPublished: vacancyRows.Count(x =>
                    x.Status == "Published" && x.ModerationStatus == ModerationStatuses.Approved),
                VacanciesPendingModeration: vacancyRows.Count(x =>
                    x.Status == "PendingModeration" || x.ModerationStatus == ModerationStatuses.Pending),
                ApplicationsTotal: applicationsByStatus.Sum(x => x.Count),
                ApplicationsNew: applicationsByStatus.FirstOrDefault(x => x.Status == "Sent")?.Count ?? 0,
                ApplicationsInProgress: applicationsByStatus.FirstOrDefault(x => x.Status == "InProgress")?.Count ?? 0,
                ApplicationsInterview: applicationsByStatus.FirstOrDefault(x => x.Status == "Interview")?.Count ?? 0,
                ApplicationsByStatus: applicationsByStatus,
                Vacancies: vacancyAnalytics);
        }
    }
}