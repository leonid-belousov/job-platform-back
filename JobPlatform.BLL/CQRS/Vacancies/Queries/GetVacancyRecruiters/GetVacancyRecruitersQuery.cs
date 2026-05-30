using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.GetVacancyRecruiters;

public sealed record GetVacancyRecruitersQuery(Guid VacancyId) : IRequest<IReadOnlyCollection<VacancyRecruiterDto>>
{
    public sealed class Handler : IRequestHandler<GetVacancyRecruitersQuery, IReadOnlyCollection<VacancyRecruiterDto>>
    {
        private readonly IApplicationDbContext _db;

        public Handler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyCollection<VacancyRecruiterDto>> Handle(GetVacancyRecruitersQuery request,
            CancellationToken cancellationToken)
        {
            return await _db.Set<VacancyRecruiter>()
                .AsNoTracking()
                .Include(x => x.RecruiterUser)
                .Where(x => x.VacancyId == request.VacancyId && x.Status == "active" && !x.IsDeleted)
                .OrderByDescending(x => x.AssignedAt)
                .Select(x => new VacancyRecruiterDto(
                    x.Id,
                    x.VacancyId,
                    x.RecruiterUserId,
                    (x.RecruiterUser.FirstName + " " + x.RecruiterUser.LastName).Trim(),
                    x.RecruiterUser.Email,
                    x.AssignedByUserId,
                    x.Status,
                    x.AssignedAt))
                .ToArrayAsync(cancellationToken);
        }
    }
}