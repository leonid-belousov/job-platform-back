using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Queries.SearchCandidates;

public sealed record SearchCandidatesQuery(
    string? Text,
    string? Skill,
    string? ExperienceLevel,
    string? Country,
    string? Language,
    string? Profession,
    string? JobSearchStatus,
    string? SortBy = "date",
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CandidateSearchItemDto>>
{
    public sealed class Handler : IRequestHandler<SearchCandidatesQuery, PagedResult<CandidateSearchItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public Handler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<CandidateSearchItemDto>> Handle(SearchCandidatesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _db.Set<CandidateProfile>()
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Languages.Where(language => !language.IsDeleted))
                .Include(x => x.Experiences.Where(experience => !experience.IsDeleted))
                .Where(x => !x.IsDeleted && x.IsVisible && x.ModerationStatus != "Rejected");

            if (!string.IsNullOrWhiteSpace(request.Text))
            {
                var text = request.Text.Trim().ToLower();
                query = query.Where(x =>
                    x.FirstName.ToLower().Contains(text) ||
                    x.LastName.ToLower().Contains(text) ||
                    (x.MiddleName != null && x.MiddleName.ToLower().Contains(text)) ||
                    (x.DesiredPosition != null && x.DesiredPosition.ToLower().Contains(text)) ||
                    (x.About != null && x.About.ToLower().Contains(text)) ||
                    x.Experiences.Any(e =>
                        e.Position.ToLower().Contains(text) ||
                        e.CompanyName.ToLower().Contains(text) ||
                        (e.Description != null && e.Description.ToLower().Contains(text))));
            }

            if (!string.IsNullOrWhiteSpace(request.Country))
            {
                var country = request.Country.Trim().ToLower();
                query = query.Where(x => x.CountryOfResidence == country || x.Citizenship == country);
            }

            if (!string.IsNullOrWhiteSpace(request.Language))
            {
                var language = request.Language.Trim().ToLower();
                query = query.Where(x => x.Languages.Any(l => l.LanguageCode == language && !l.IsDeleted));
            }

            if (!string.IsNullOrWhiteSpace(request.Profession))
            {
                var profession = request.Profession.Trim().ToLower();
                query = query.Where(x => x.DesiredPosition != null && x.DesiredPosition.ToLower().Contains(profession));
            }

            if (!string.IsNullOrWhiteSpace(request.JobSearchStatus))
            {
                var status = request.JobSearchStatus.Trim().ToLower();
                query = query.Where(x => x.JobSearchStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(request.ExperienceLevel))
            {
                var level = request.ExperienceLevel.Trim().ToLower();
                query = level switch
                {
                    "no_experience" => query.Where(x => !x.Experiences.Any(e => !e.IsDeleted)),
                    _ => query.Where(x => x.Experiences.Any(e => !e.IsDeleted))
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Skill))
            {
                var skill = request.Skill.Trim().ToLower();
                query = query.Where(x =>
                    (x.About != null && x.About.ToLower().Contains(skill)) ||
                    (x.DesiredPosition != null && x.DesiredPosition.ToLower().Contains(skill)) ||
                    x.Experiences.Any(e =>
                        e.Position.ToLower().Contains(skill) ||
                        (e.Description != null && e.Description.ToLower().Contains(skill))));
            }

            query = request.SortBy?.Trim().ToLower() switch
            {
                "profession" => query.OrderBy(x => x.DesiredPosition).ThenByDescending(x => x.CreatedAt),
                "country" => query.OrderBy(x => x.CountryOfResidence).ThenByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new CandidateSearchItemDto(
                    x.Id,
                    (x.FirstName + " " + x.LastName).Trim(),
                    x.DateOfBirth,
                    x.Citizenship,
                    x.CountryOfResidence,
                    x.City,
                    x.DesiredPosition,
                    x.ExpectedSalary,
                    x.Currency,
                    x.JobSearchStatus,
                    x.ModerationStatus,
                    x.Languages
                        .Where(l => !l.IsDeleted)
                        .OrderBy(l => l.LanguageCode)
                        .Select(l => new CandidateLanguageDto(l.Id, l.LanguageCode, l.Level))
                        .ToArray(),
                    false,
                    null,
                    null,
                    x.CreatedAt))
                .ToArrayAsync(cancellationToken);

            return new PagedResult<CandidateSearchItemDto>(items, total, request.Page, request.PageSize);
        }
    }
}