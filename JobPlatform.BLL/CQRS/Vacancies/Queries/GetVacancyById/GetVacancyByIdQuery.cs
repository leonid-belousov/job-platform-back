using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.GetVacancyById;

public sealed record GetVacancyByIdQuery(Guid VacancyId) : IRequest<VacancyDto?>
{
    public sealed class GetVacancyByIdQueryHandler : IRequestHandler<GetVacancyByIdQuery, VacancyDto?>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetVacancyByIdQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<VacancyDto?> Handle(GetVacancyByIdQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<JobVacancy>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Id == request.VacancyId && x.Status == "Published")
                .Select(x => new VacancyDto(
                    x.Id,
                    x.Title,
                    x.City,
                    x.EmploymentType,
                    x.WorkFormat,
                    x.ExperienceLevel,
                    x.SalaryFrom,
                    x.SalaryTo,
                    x.Currency,
                    x.Status,
                    x.ModerationStatus,
                    x.ModerationComment,
                    x.ModeratedByUserId,
                    x.ModeratedAt,
                    x.PublishedAt,
                    x.ExpiresAt,
                    x.ExtendedAt,
                    x.ExtensionCount,
                    x.Description,
                    x.Requirements,
                    x.Responsibilities,
                    x.Conditions,
                    x.Country))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
