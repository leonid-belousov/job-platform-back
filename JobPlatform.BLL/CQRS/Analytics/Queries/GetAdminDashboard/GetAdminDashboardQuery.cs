using JobPlatform.BLL.CQRS.Analytics.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.Core.Entities.Users;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Analytics.Queries.GetAdminDashboard;

public sealed record GetAdminDashboardQuery : IRequest<AdminDashboardDto>
{
    public sealed class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
    {
        private readonly IApplicationDbContext _db;
        public GetAdminDashboardQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            var applicationsByStatus = await _db.Set<JobApplication>()
                .AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(x => new StatusCountDto(x.Key, x.Count()))
                .OrderBy(x => x.Status)
                .ToArrayAsync(cancellationToken);

            return new AdminDashboardDto(
                TotalUsers: await _db.Set<User>().AsNoTracking().CountAsync(x => !x.IsDeleted, cancellationToken),
                ActiveUsers: await _db.Set<User>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.Status == UserStatus.Active, cancellationToken),
                BlockedUsers: await _db.Set<User>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.Status == UserStatus.Blocked, cancellationToken),
                CandidateProfiles: await _db.Set<CandidateProfile>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted, cancellationToken),
                CompaniesTotal: await _db.Set<Company>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted, cancellationToken),
                CompaniesPendingModeration: await _db.Set<Company>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.ModerationStatus == ModerationStatuses.Pending,
                        cancellationToken),
                CompaniesApproved: await _db.Set<Company>().AsNoTracking().CountAsync(
                    x => !x.IsDeleted && x.ModerationStatus == ModerationStatuses.Approved, cancellationToken),
                CompaniesRejected: await _db.Set<Company>().AsNoTracking().CountAsync(
                    x => !x.IsDeleted && x.ModerationStatus == ModerationStatuses.Rejected, cancellationToken),
                VacanciesTotal: await _db.Set<JobVacancy>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted, cancellationToken),
                VacanciesPublished: await _db.Set<JobVacancy>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.Status == "Published", cancellationToken),
                VacanciesPendingModeration: await _db.Set<JobVacancy>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.Status == "PendingModeration", cancellationToken),
                VacanciesArchived: await _db.Set<JobVacancy>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.Status == "Archived", cancellationToken),
                VacanciesRejected: await _db.Set<JobVacancy>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.Status == "Rejected", cancellationToken),
                ApplicationsTotal: await _db.Set<JobApplication>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted, cancellationToken),
                ApplicationsByStatus: applicationsByStatus,
                CrmLeadsTotal: await _db.Set<CrmLead>().AsNoTracking().CountAsync(x => !x.IsDeleted, cancellationToken),
                CrmOpenTasks: await _db.Set<CrmTask>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted && x.Status == "Open", cancellationToken),
                CrmOverdueTasks: await _db.Set<CrmTask>().AsNoTracking().CountAsync(
                    x => !x.IsDeleted && x.Status == "Open" && x.DueDate.HasValue && x.DueDate.Value < now,
                    cancellationToken),
                QuestionnaireResponsesTotal: await _db.Set<QuestionnaireResponse>().AsNoTracking()
                    .CountAsync(x => !x.IsDeleted, cancellationToken),
                GeneratedAt: now);
        }
    }
}