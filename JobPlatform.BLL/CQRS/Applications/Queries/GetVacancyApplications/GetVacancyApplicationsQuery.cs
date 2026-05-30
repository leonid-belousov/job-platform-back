using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.GetVacancyApplications;

public sealed record GetVacancyApplicationsQuery(Guid VacancyId) : IRequest<IReadOnlyCollection<ApplicationDto>>
{
    public class
        GetVacancyApplicationsQueryHandler : IRequestHandler<GetVacancyApplicationsQuery,
        IReadOnlyCollection<ApplicationDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetVacancyApplicationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<ApplicationDto>> Handle(GetVacancyApplicationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var vacancy = await _db.Set<JobVacancy>().AsNoTracking()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Вакансия не найдена.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к откликам этой вакансии.");

            var applications = await _db.Set<JobApplication>()
                .AsNoTracking()
                .Include(x => x.Vacancy)
                .Include(x => x.CandidateProfile)
                .ThenInclude(x => x.User)
                .Where(x => x.VacancyId == request.VacancyId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToArrayAsync(cancellationToken);

            return applications.Select(x =>
            {
                var contactsVisible = ApplicationStatuses.CanSeeContacts(x.Status);
                var candidateName = $"{x.CandidateProfile.FirstName} {x.CandidateProfile.LastName}".Trim();

                return new ApplicationDto(
                    x.Id,
                    x.VacancyId,
                    x.Vacancy.Title,
                    x.CandidateProfileId,
                    candidateName,
                    contactsVisible ? x.CandidateProfile.User.Email : null,
                    contactsVisible ? x.CandidateProfile.Phone : null,
                    contactsVisible,
                    x.ResumeId,
                    x.Status,
                    x.CoverLetter,
                    x.CreatedAt);
            }).ToArray();
        }
    }
}