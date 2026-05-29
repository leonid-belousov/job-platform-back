using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.GetMyCompanyVacancies;

public sealed record GetMyCompanyVacanciesQuery(Guid CompanyId) : IRequest<IReadOnlyCollection<VacancyDto>>
{
    public class
        GetMyCompanyVacanciesQueryHandler : IRequestHandler<GetMyCompanyVacanciesQuery, IReadOnlyCollection<VacancyDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetMyCompanyVacanciesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<VacancyDto>> Handle(GetMyCompanyVacanciesQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == request.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к компании.");

            return await _db.Set<JobVacancy>()
                .AsNoTracking()
                .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new VacancyDto(x.Id, x.Title, x.City, x.SalaryFrom, x.SalaryTo, x.Status))
                .ToArrayAsync(cancellationToken);
        }
    }
}