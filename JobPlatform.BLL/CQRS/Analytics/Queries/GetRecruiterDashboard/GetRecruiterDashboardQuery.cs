using JobPlatform.BLL.CQRS.Analytics.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Analytics.Queries.GetRecruiterDashboard;

public sealed record GetRecruiterDashboardQuery : IRequest<RecruiterDashboardDto>
{
    public sealed class
        GetRecruiterDashboardQueryHandler : IRequestHandler<GetRecruiterDashboardQuery, RecruiterDashboardDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetRecruiterDashboardQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<RecruiterDashboardDto> Handle(GetRecruiterDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var now = DateTimeOffset.UtcNow;

            var leadQuery = _db.Set<CrmLead>().AsNoTracking().Where(x => !x.IsDeleted && x.ResponsibleUserId == userId);
            var taskQuery = _db.Set<CrmTask>().AsNoTracking().Where(x => !x.IsDeleted && x.ResponsibleUserId == userId);
            var vacancyQuery = _db.Set<JobVacancy>().AsNoTracking()
                .Where(x => !x.IsDeleted && x.CreatedByUserId == userId);

            var managedVacancyIds = await vacancyQuery.Select(x => x.Id).ToArrayAsync(cancellationToken);
            var applicationsQuery = _db.Set<JobApplication>().AsNoTracking()
                .Where(x => !x.IsDeleted && managedVacancyIds.Contains(x.VacancyId));

            var leadsByStatus = await leadQuery.GroupBy(x => x.Status)
                .Select(x => new StatusCountDto(x.Key, x.Count()))
                .OrderBy(x => x.Status)
                .ToArrayAsync(cancellationToken);

            var tasksByStatus = await taskQuery.GroupBy(x => x.Status)
                .Select(x => new StatusCountDto(x.Key, x.Count()))
                .OrderBy(x => x.Status)
                .ToArrayAsync(cancellationToken);

            var applicationsByStatus = await applicationsQuery.GroupBy(x => x.Status)
                .Select(x => new StatusCountDto(x.Key, x.Count()))
                .OrderBy(x => x.Status)
                .ToArrayAsync(cancellationToken);

            return new RecruiterDashboardDto(
                AssignedCrmLeadsTotal: leadsByStatus.Sum(x => x.Count),
                AssignedCrmLeadsInProgress: leadsByStatus.FirstOrDefault(x => x.Status == "InProgress")?.Count ?? 0,
                AssignedOpenTasks: tasksByStatus.FirstOrDefault(x => x.Status == "Open")?.Count ?? 0,
                AssignedOverdueTasks: await taskQuery.CountAsync(
                    x => x.Status == "Open" && x.DueDate.HasValue && x.DueDate.Value < now, cancellationToken),
                ManagedVacanciesTotal: await vacancyQuery.CountAsync(cancellationToken),
                ManagedVacanciesPublished: await vacancyQuery.CountAsync(
                    x => x.Status == "Published" && x.ModerationStatus == ModerationStatuses.Approved,
                    cancellationToken),
                ManagedApplicationsTotal: applicationsByStatus.Sum(x => x.Count),
                CrmLeadsByStatus: leadsByStatus,
                TasksByStatus: tasksByStatus,
                ApplicationsByStatus: applicationsByStatus,
                GeneratedAt: now);
        }
    }
}