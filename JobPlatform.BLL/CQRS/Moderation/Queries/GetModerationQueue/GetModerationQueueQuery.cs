using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Moderation.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Moderation.Queries.GetModerationQueue;

public sealed record GetModerationQueueQuery(
    string? EntityType,
    string? ModerationStatus,
    string? Search,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<ModerationQueueItemDto>>
{
    public class
        GetModerationQueueQueryHandler : IRequestHandler<GetModerationQueueQuery, PagedResult<ModerationQueueItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetModerationQueueQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<ModerationQueueItemDto>> Handle(GetModerationQueueQuery request,
            CancellationToken cancellationToken)
        {
            var items = new List<ModerationQueueItemDto>();

            if (string.IsNullOrWhiteSpace(request.EntityType) || request.EntityType == "company")
            {
                var companies = _db.Set<Company>().AsNoTracking().Where(x => !x.IsDeleted);
                if (!string.IsNullOrWhiteSpace(request.ModerationStatus))
                    companies = companies.Where(x => x.ModerationStatus == request.ModerationStatus);
                if (!string.IsNullOrWhiteSpace(request.Search))
                    companies = companies.Where(x => x.Name.ToLower().Contains(request.Search.ToLower()));

                items.AddRange(await companies.Select(x => new ModerationQueueItemDto(
                    x.Id,
                    "company",
                    x.Name,
                    x.ModerationStatus,
                    x.Status,
                    x.ModerationComment,
                    x.ModeratedByUserId,
                    x.ModeratedAt,
                    x.CreatedAt,
                    x.UpdatedAt)).ToListAsync(cancellationToken));
            }

            if (string.IsNullOrWhiteSpace(request.EntityType) || request.EntityType == "vacancy")
            {
                var vacancies = _db.Set<JobVacancy>().AsNoTracking().Where(x => !x.IsDeleted);
                if (!string.IsNullOrWhiteSpace(request.ModerationStatus))
                    vacancies = vacancies.Where(x => x.ModerationStatus == request.ModerationStatus);
                if (!string.IsNullOrWhiteSpace(request.Search))
                    vacancies = vacancies.Where(x => x.Title.ToLower().Contains(request.Search.ToLower()));

                items.AddRange(await vacancies.Select(x => new ModerationQueueItemDto(
                    x.Id,
                    "vacancy",
                    x.Title,
                    x.ModerationStatus,
                    x.Status,
                    x.ModerationComment,
                    x.ModeratedByUserId,
                    x.ModeratedAt,
                    x.CreatedAt,
                    x.UpdatedAt)).ToListAsync(cancellationToken));
            }

            var ordered = items.OrderByDescending(x => x.CreatedAt).ToList();
            var total = ordered.Count;
            var pageItems = ordered.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
            return new PagedResult<ModerationQueueItemDto>(pageItems, total, request.Page, request.PageSize);
        }
    }
}