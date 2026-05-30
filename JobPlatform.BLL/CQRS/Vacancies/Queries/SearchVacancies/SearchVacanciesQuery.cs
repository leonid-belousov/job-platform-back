using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.SearchVacancies;

public sealed record SearchVacanciesQuery(
    string? Text,
    string? City,
    string? EmploymentType,
    string? WorkFormat,
    string? ExperienceLevel,
    string? Currency,
    int Page = 1,
    int PageSize = 20)
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
                var text = request.Text.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(text) || x.Description.ToLower().Contains(text));
            }

            if (!string.IsNullOrWhiteSpace(request.City)) query = query.Where(x => x.City == request.City);
            if (!string.IsNullOrWhiteSpace(request.EmploymentType))
                query = query.Where(x => x.EmploymentType == request.EmploymentType);
            if (!string.IsNullOrWhiteSpace(request.WorkFormat))
                query = query.Where(x => x.WorkFormat == request.WorkFormat);
            if (!string.IsNullOrWhiteSpace(request.ExperienceLevel))
                query = query.Where(x => x.ExperienceLevel == request.ExperienceLevel);
            if (!string.IsNullOrWhiteSpace(request.Currency)) query = query.Where(x => x.Currency == request.Currency);

            return await query.OrderByDescending(x => x.PublishedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new VacancyDto(x.Id, x.Title, x.City, x.EmploymentType, x.WorkFormat, x.ExperienceLevel,
                    x.SalaryFrom, x.SalaryTo, x.Currency, x.Status))
                .ToArrayAsync(cancellationToken);
        }
    }
}