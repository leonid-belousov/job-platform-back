using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Moderation.DTO;
using JobPlatform.Core.Entities.Candidates;
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
            var entityType = request.EntityType?.Trim().ToLowerInvariant();
            var moderationStatus = request.ModerationStatus?.Trim();
            var search = request.Search?.Trim().ToLowerInvariant();
            var items = new List<ModerationQueueItemDto>();

            if (string.IsNullOrWhiteSpace(entityType) || entityType == "company")
            {
                var companies = _db.Set<Company>().AsNoTracking().Where(x => !x.IsDeleted);
                if (!string.IsNullOrWhiteSpace(moderationStatus))
                    companies = companies.Where(x => x.ModerationStatus == moderationStatus);
                if (!string.IsNullOrWhiteSpace(search))
                    companies = companies.Where(x => x.Name.ToLower().Contains(search));

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

            if (string.IsNullOrWhiteSpace(entityType) || entityType == "vacancy")
            {
                var vacancies = _db.Set<JobVacancy>().AsNoTracking().Where(x => !x.IsDeleted);
                if (!string.IsNullOrWhiteSpace(moderationStatus))
                    vacancies = vacancies.Where(x => x.ModerationStatus == moderationStatus);
                if (!string.IsNullOrWhiteSpace(search))
                    vacancies = vacancies.Where(x => x.Title.ToLower().Contains(search));

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

            if (string.IsNullOrWhiteSpace(entityType) || entityType == "candidate")
            {
                var candidates = _db.Set<CandidateProfile>().AsNoTracking().Where(x => !x.IsDeleted);
                if (!string.IsNullOrWhiteSpace(moderationStatus))
                    candidates = candidates.Where(x => x.ModerationStatus == moderationStatus);
                if (!string.IsNullOrWhiteSpace(search))
                    candidates = candidates.Where(x =>
                        x.FirstName.ToLower().Contains(search) ||
                        x.LastName.ToLower().Contains(search) ||
                        (x.MiddleName != null && x.MiddleName.ToLower().Contains(search)) ||
                        (x.DesiredPosition != null && x.DesiredPosition.ToLower().Contains(search)) ||
                        (x.CountryOfResidence != null && x.CountryOfResidence.ToLower().Contains(search)) ||
                        (x.Citizenship != null && x.Citizenship.ToLower().Contains(search)));

                items.AddRange(await candidates.Select(x => new ModerationQueueItemDto(
                    x.Id,
                    "candidate",
                    (x.FirstName + " " + x.LastName).Trim(),
                    x.ModerationStatus,
                    x.JobSearchStatus,
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