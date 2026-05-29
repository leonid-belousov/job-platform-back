using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.SearchVacancies;

public sealed record SearchVacanciesQuery(string? Text, string? City, int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyCollection<VacancyDto>>
{
    public class SearchVacanciesQueryHandler : IRequestHandler<SearchVacanciesQuery, IReadOnlyCollection<VacancyDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public SearchVacanciesQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<VacancyDto>> Handle(SearchVacanciesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<JobVacancy>().AsNoTracking().Where(x => !x.IsDeleted && x.Status == "Published");

            if (!string.IsNullOrWhiteSpace(request.Text))
            {
                query = query.Where(x =>
                    x.Title.ToLower().Contains(request.Text.ToLower()) ||
                    x.Description.ToLower().Contains(request.Text.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                query = query.Where(x => x.City == request.City);
            }

            return await query.OrderByDescending(x => x.PublishedAt).Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new VacancyDto(x.Id, x.Title, x.City, x.SalaryFrom, x.SalaryTo, x.Status))
                .ToArrayAsync(cancellationToken);
        }
    }
}