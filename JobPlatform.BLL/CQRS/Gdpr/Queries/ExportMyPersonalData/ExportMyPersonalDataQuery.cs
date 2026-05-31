using JobPlatform.BLL.CQRS.Gdpr.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Legal;
using JobPlatform.Core.Entities.Notifications;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Gdpr.Queries.ExportMyPersonalData;

public sealed record ExportMyPersonalDataQuery : IRequest<PersonalDataExportDto>
{
    public sealed class Handler : IRequestHandler<ExportMyPersonalDataQuery, PersonalDataExportDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<PersonalDataExportDto> Handle(ExportMyPersonalDataQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var user = await _db.Set<User>().AsNoTracking()
                           .Include(x => x.UserRoles)
                           .ThenInclude(x => x.Role)
                           .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("User not found.");

            var profile = await _db.Set<CandidateProfile>().AsNoTracking()
                .Include(x => x.Languages.Where(language => !language.IsDeleted))
                .Include(x => x.Experiences.Where(experience => !experience.IsDeleted))
                .Include(x => x.Skills.Where(skill => !skill.IsDeleted))
                .Include(x => x.Resumes.Where(resume => !resume.IsDeleted))
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);

            var legalConsents = await _db.Set<UserLegalConsent>().AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.AcceptedAt)
                .Select(x => new
                {
                    x.DocumentType,
                    x.Version,
                    x.Language,
                    x.AcceptedAt,
                    x.IpAddress
                })
                .ToArrayAsync(cancellationToken);

            var applications = profile is null
                ? Array.Empty<object>()
                : await _db.Set<JobApplication>().AsNoTracking()
                    .Include(x => x.Vacancy)
                    .Where(x => x.CandidateProfileId == profile.Id && !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.VacancyId,
                        VacancyTitle = x.Vacancy.Title,
                        x.ResumeId,
                        x.Status,
                        x.CoverLetter,
                        x.CreatedAt,
                        x.UpdatedAt
                    })
                    .Cast<object>()
                    .ToArrayAsync(cancellationToken);

            var notifications = await _db.Set<Notification>().AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Type,
                    x.Title,
                    x.Message,
                    x.EntityType,
                    x.EntityId,
                    x.ReadAt,
                    x.CreatedAt
                })
                .Cast<object>()
                .ToArrayAsync(cancellationToken);

            var userData = new
            {
                user.Id,
                user.Email,
                user.Phone,
                user.FirstName,
                user.LastName,
                user.EmailConfirmed,
                user.PhoneConfirmed,
                user.Status,
                user.LastLoginAt,
                Roles = user.UserRoles.Select(x => x.Role.Code).ToArray(),
                user.CreatedAt,
                user.UpdatedAt
            };

            object? candidateData = profile is null
                ? null
                : new
                {
                    profile.Id,
                    profile.FirstName,
                    profile.MiddleName,
                    profile.LastName,
                    profile.DateOfBirth,
                    profile.Citizenship,
                    profile.CountryOfResidence,
                    profile.City,
                    profile.Phone,
                    profile.DesiredPosition,
                    profile.ExpectedSalary,
                    profile.Currency,
                    profile.About,
                    profile.IsVisible,
                    profile.JobSearchStatus,
                    profile.HasNoExperience,
                    profile.IsComplete,
                    profile.CompletedAt,
                    profile.ModerationStatus,
                    profile.ModerationComment,
                    profile.CreatedAt,
                    profile.UpdatedAt,
                    Languages = profile.Languages.Select(x => new { x.LanguageCode, x.Level }).ToArray(),
                    Experiences = profile.Experiences.Select(x => new { x.CompanyName, x.Position, x.StartDate, x.EndDate, x.Description }).ToArray(),
                    Skills = profile.Skills.Select(x => new { x.SkillCode, x.Name }).ToArray(),
                    Resumes = profile.Resumes.Select(x => new { x.Id, x.Title, x.FileId, x.Status, x.IsActive, x.CreatedAt }).ToArray()
                };

            return new PersonalDataExportDto(userData, candidateData, legalConsents, applications, notifications,
                DateTimeOffset.UtcNow);
        }
    }
}
