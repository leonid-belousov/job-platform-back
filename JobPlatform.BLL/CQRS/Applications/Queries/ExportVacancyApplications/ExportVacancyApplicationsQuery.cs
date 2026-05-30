using System.Globalization;
using System.Text;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.ExportVacancyApplications;

public sealed record ExportVacancyApplicationsQuery(Guid VacancyId) : IRequest<byte[]>
{
    public sealed class Handler : IRequestHandler<ExportVacancyApplicationsQuery, byte[]>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<byte[]> Handle(ExportVacancyApplicationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var vacancy = await _db.Set<JobApplication>()
                .AsNoTracking()
                .Where(x => x.VacancyId == request.VacancyId)
                .Select(x => x.Vacancy)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Vacancy not found or has no applications.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException();
            }

            var rows = await _db.Set<JobApplication>()
                .AsNoTracking()
                .Include(x => x.CandidateProfile)
                .Where(x => x.VacancyId == request.VacancyId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    CandidateName = (x.CandidateProfile.FirstName + " " + x.CandidateProfile.LastName).Trim(),
                    x.CandidateProfile.DesiredPosition,
                    x.CandidateProfile.CountryOfResidence,
                    x.CandidateProfile.City,
                    x.CoverLetter,
                    x.CreatedAt
                })
                .ToArrayAsync(cancellationToken);

            var csv = new StringBuilder();
            csv.AppendLine("Id,Status,CandidateName,DesiredPosition,CountryOfResidence,City,CoverLetter,CreatedAt");

            foreach (var row in rows)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    Escape(row.Id.ToString()),
                    Escape(row.Status),
                    Escape(row.CandidateName),
                    Escape(row.DesiredPosition),
                    Escape(row.CountryOfResidence),
                    Escape(row.City),
                    Escape(row.CoverLetter),
                    Escape(row.CreatedAt.ToString("O", CultureInfo.InvariantCulture))
                }));
            }

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        }

        private static string Escape(string? value)
        {
            var normalized = (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
            return normalized.Contains(',') || normalized.Contains('"')
                ? "\"" + normalized.Replace("\"", "\"\"") + "\""
                : normalized;
        }
    }
}