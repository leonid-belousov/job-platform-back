using System.Text;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExportController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public ExportController(IApplicationDbContext db)
    {
        _db = db;
    }

    [Authorize(Policy = "CandidatesRead")]
    [HttpGet("candidates.csv")]
    public async Task<IActionResult> ExportCandidates(CancellationToken cancellationToken)
    {
        var rows = await _db.Set<CandidateProfile>()
            .AsNoTracking()
            .Include(x => x.Languages.Where(l => !l.IsDeleted))
            .Include(x => x.Experiences.Where(e => !e.IsDeleted))
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new[]
            {
                x.Id.ToString(),
                x.FirstName,
                x.MiddleName ?? string.Empty,
                x.LastName,
                x.DateOfBirth.HasValue ? x.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                x.Citizenship ?? string.Empty,
                x.CountryOfResidence ?? string.Empty,
                x.City ?? string.Empty,
                x.DesiredPosition ?? string.Empty,
                x.ExpectedSalary.HasValue ? x.ExpectedSalary.Value.ToString() : string.Empty,
                x.Currency ?? string.Empty,
                x.JobSearchStatus,
                x.ModerationStatus,
                string.Join(";", x.Languages.Select(l => l.LanguageCode + ":" + l.Level)),
                string.Join(";", x.Experiences.Select(e => e.Position + " at " + e.CompanyName)),
                x.CreatedAt.ToString("O")
            })
            .ToArrayAsync(cancellationToken);

        var csv = ToCsv(new[]
        {
            "Id", "FirstName", "MiddleName", "LastName", "DateOfBirth", "Citizenship", "CountryOfResidence",
            "City", "DesiredPosition", "ExpectedSalary", "Currency", "JobSearchStatus", "ModerationStatus",
            "Languages", "Experience", "CreatedAt"
        }, rows);

        return File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", "candidates.csv");
    }

    [Authorize(Policy = "ApplicationsManage")]
    [HttpGet("vacancies/{vacancyId:guid}/applications.csv")]
    public async Task<IActionResult> ExportVacancyApplications(Guid vacancyId, CancellationToken cancellationToken)
    {
        var rows = await _db.Set<JobApplication>()
            .AsNoTracking()
            .Include(x => x.Vacancy)
            .Include(x => x.CandidateProfile)
            .Where(x => x.VacancyId == vacancyId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new[]
            {
                x.Id.ToString(),
                x.VacancyId.ToString(),
                x.Vacancy.Title,
                x.CandidateProfileId.ToString(),
                x.CandidateProfile.FirstName + " " + x.CandidateProfile.LastName,
                x.Status,
                x.CoverLetter ?? string.Empty,
                x.CreatedAt.ToString("O")
            })
            .ToArrayAsync(cancellationToken);

        var csv = ToCsv(new[]
        {
            "Id", "VacancyId", "VacancyTitle", "CandidateProfileId", "CandidateName", "Status", "CoverLetter", "CreatedAt"
        }, rows);

        return File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", "vacancy-applications.csv");
    }

    private static string ToCsv(IReadOnlyCollection<string> headers, IReadOnlyCollection<string[]> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', headers.Select(Escape)));
        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',', row.Select(Escape)));
        }

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        value ??= string.Empty;
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}