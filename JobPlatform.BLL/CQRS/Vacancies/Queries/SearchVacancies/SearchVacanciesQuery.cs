using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Queries.SearchVacancies;

public sealed record SearchVacanciesQuery(
    string? Text,
    string? Country,
    string? City,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string? EmploymentType,
    string? WorkFormat,
    string? ExperienceLevel,
    string? Currency,
    string? SortBy = "date",
    int Page = 1,
    int PageSize = 20)
    : IRequest<PagedResult<VacancyDto>>
{
    public class SearchVacanciesQueryHandler : IRequestHandler<SearchVacanciesQuery, PagedResult<VacancyDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public SearchVacanciesQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<VacancyDto>> Handle(SearchVacanciesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<JobVacancy>().AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status == "Published");

            var text = request.Text?.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(text))
            {
                query = query.Where(x =>
                    x.Title.ToLower().Contains(text) ||
                    x.Description.ToLower().Contains(text) ||
                    (x.Requirements != null && x.Requirements.ToLower().Contains(text)) ||
                    (x.Responsibilities != null && x.Responsibilities.ToLower().Contains(text)) ||
                    (x.Conditions != null && x.Conditions.ToLower().Contains(text)));
            }

            if (!string.IsNullOrWhiteSpace(request.Country))
            {
                var country = request.Country.Trim().ToLower();
                query = query.Where(x => x.City != null && x.City.ToLower().Contains(country));
            }

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var city = request.City.Trim().ToLower();
                query = query.Where(x => x.City != null && x.City.ToLower() == city);
            }

            if (request.SalaryFrom.HasValue)
            {
                query = query.Where(x => !x.SalaryTo.HasValue || x.SalaryTo >= request.SalaryFrom.Value);
            }

            if (request.SalaryTo.HasValue)
            {
                query = query.Where(x => !x.SalaryFrom.HasValue || x.SalaryFrom <= request.SalaryTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.EmploymentType))
                query = query.Where(x => x.EmploymentType == request.EmploymentType);
            if (!string.IsNullOrWhiteSpace(request.WorkFormat))
                query = query.Where(x => x.WorkFormat == request.WorkFormat);
            if (!string.IsNullOrWhiteSpace(request.ExperienceLevel))
                query = query.Where(x => x.ExperienceLevel == request.ExperienceLevel);
            if (!string.IsNullOrWhiteSpace(request.Currency)) query = query.Where(x => x.Currency == request.Currency);

            query = request.SortBy?.Trim().ToLower() switch
            {
                "salary" => query.OrderByDescending(x => x.SalaryTo ?? x.SalaryFrom ?? 0).ThenByDescending(x => x.PublishedAt),
                "relevance" when !string.IsNullOrWhiteSpace(text) => query
                    .OrderByDescending(x => x.Title.ToLower().Contains(text))
                    .ThenByDescending(x => x.PublishedAt),
                _ => query.OrderByDescending(x => x.PublishedAt)
            };

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new VacancyDto(x.Id, x.Title, x.City, x.EmploymentType, x.WorkFormat, x.ExperienceLevel,
                    x.SalaryFrom, x.SalaryTo, x.Currency, x.Status, x.ModerationStatus, x.ModerationComment,
                    x.ModeratedByUserId, x.ModeratedAt))
                .ToArrayAsync(cancellationToken);

            return new PagedResult<VacancyDto>(items, total, request.Page, request.PageSize);
        }
    }
}