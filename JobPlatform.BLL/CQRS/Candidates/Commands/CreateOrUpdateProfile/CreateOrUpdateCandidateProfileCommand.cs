using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateOrUpdateProfile;

public sealed record CandidateLanguageRequest(string LanguageCode, string Level);

public sealed record CandidateExperienceRequest(
    string CompanyName,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description);

public sealed record CandidateEducationRequest(
    string InstitutionName,
    string? Faculty,
    string? Degree,
    int? StartYear,
    int? EndYear);

public sealed record CandidateCertificateRequest(
    string Name,
    string? Issuer,
    DateOnly? IssueDate,
    DateOnly? ExpirationDate,
    string? CredentialId,
    string? CredentialUrl);

public sealed record CandidateSkillRequest(string SkillCode, string? Name);

public sealed record CreateOrUpdateCandidateProfileCommand(
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly DateOfBirth,
    string Citizenship,
    string CountryOfResidence,
    string? City,
    string? Phone,
    string DesiredPosition,
    decimal? ExpectedSalary,
    string? Currency,
    string JobSearchStatus,
    string? About,
    Guid? PhotoFileId,
    string? PhotoUrl,
    bool IsVisible,
    bool HasNoExperience,
    IReadOnlyCollection<CandidateLanguageRequest> Languages,
    IReadOnlyCollection<CandidateExperienceRequest> Experiences,
    IReadOnlyCollection<CandidateEducationRequest> Educations,
    IReadOnlyCollection<CandidateCertificateRequest> Certificates,
    IReadOnlyCollection<CandidateSkillRequest> Skills) : IRequest<CandidateProfileDto>
{
    public class
        CreateOrUpdateCandidateProfileCommandHandler : IRequestHandler<CreateOrUpdateCandidateProfileCommand,
        CandidateProfileDto>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public CreateOrUpdateCandidateProfileCommandHandler(IApplicationDbContext dbContext,
            ICurrentUserService currentUserService, IAuditService auditService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        public async Task<CandidateProfileDto> Handle(CreateOrUpdateCandidateProfileCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
            var profile = await _dbContext.Set<CandidateProfile>()
                .Include(p => p.Languages.Where(x => !x.IsDeleted))
                .Include(p => p.Experiences.Where(x => !x.IsDeleted))
                .Include(p => p.Educations.Where(x => !x.IsDeleted))
                .Include(p => p.Certificates.Where(x => !x.IsDeleted))
                .Include(p => p.Skills.Where(x => !x.IsDeleted))
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);

            var oldValue = profile is null
                ? null
                : new
                {
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
                    profile.PhotoFileId,
                    profile.PhotoUrl,
                    profile.IsVisible,
                    profile.JobSearchStatus,
                    profile.HasNoExperience,
                    profile.IsComplete,
                    Languages = profile.Languages.Select(x => new { x.LanguageCode, x.Level }).ToArray(),
                    Experiences = profile.Experiences.Select(x => new { x.CompanyName, x.Position, x.StartDate, x.EndDate }).ToArray(),
                    Educations = profile.Educations.Select(x => new { x.InstitutionName, x.Faculty, x.Degree, x.StartYear, x.EndYear }).ToArray(),
                    Certificates = profile.Certificates.Select(x => new { x.Name, x.Issuer, x.IssueDate, x.ExpirationDate, x.CredentialId, x.CredentialUrl }).ToArray(),
                    Skills = profile.Skills.Select(x => new { x.SkillCode, x.Name }).ToArray()
                };

            if (profile is null)
            {
                profile = new CandidateProfile
                {
                    UserId = userId
                };
                _dbContext.Set<CandidateProfile>().Add(profile);
            }

            profile.FirstName = request.FirstName.Trim();
            profile.MiddleName = string.IsNullOrWhiteSpace(request.MiddleName) ? null : request.MiddleName.Trim();
            profile.LastName = request.LastName.Trim();
            profile.DateOfBirth = request.DateOfBirth;
            profile.Citizenship = request.Citizenship.Trim().ToLowerInvariant();
            profile.CountryOfResidence = request.CountryOfResidence.Trim().ToLowerInvariant();
            profile.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim().ToLowerInvariant();
            profile.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
            profile.DesiredPosition = request.DesiredPosition.Trim();
            profile.ExpectedSalary = request.ExpectedSalary;
            profile.Currency = string.IsNullOrWhiteSpace(request.Currency) ? null : request.Currency.Trim().ToLowerInvariant();
            profile.JobSearchStatus = request.JobSearchStatus.Trim().ToLowerInvariant();
            profile.About = string.IsNullOrWhiteSpace(request.About) ? null : request.About.Trim();
            profile.PhotoFileId = request.PhotoFileId;
            profile.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();
            profile.IsVisible = request.IsVisible;
            profile.HasNoExperience = request.HasNoExperience;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            ReplaceLanguages(profile, request.Languages);
            ReplaceExperiences(profile, request.Experiences, request.HasNoExperience);
            ReplaceEducations(profile, request.Educations);
            ReplaceCertificates(profile, request.Certificates);
            await ReplaceSkillsAsync(profile, request.Skills, cancellationToken);

            var isComplete = IsProfileComplete(profile);
            profile.IsComplete = isComplete;
            profile.CompletedAt = isComplete ? profile.CompletedAt ?? DateTimeOffset.UtcNow : null;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.CandidateProfileUpdated,
                EntityType: nameof(CandidateProfile),
                EntityId: profile.Id,
                OldValue: oldValue,
                NewValue: new
                {
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
                    profile.PhotoFileId,
                    profile.PhotoUrl,
                    profile.IsVisible,
                    profile.JobSearchStatus,
                    profile.HasNoExperience,
                    profile.IsComplete,
                    profile.CompletedAt,
                    Languages = profile.Languages.Select(x => new { x.LanguageCode, x.Level }).ToArray(),
                    Experiences = profile.Experiences.Select(x => new { x.CompanyName, x.Position, x.StartDate, x.EndDate }).ToArray(),
                    Educations = profile.Educations.Select(x => new { x.InstitutionName, x.Faculty, x.Degree, x.StartYear, x.EndYear }).ToArray(),
                    Certificates = profile.Certificates.Select(x => new { x.Name, x.Issuer, x.IssueDate, x.ExpirationDate, x.CredentialId, x.CredentialUrl }).ToArray(),
                    Skills = profile.Skills.Select(x => new { x.SkillCode, x.Name }).ToArray()
                },
                UserId: userId), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ToDto(profile);
        }

        private void ReplaceLanguages(CandidateProfile profile, IReadOnlyCollection<CandidateLanguageRequest> languages)
        {
            if (profile.Languages.Count > 0)
            {
                _dbContext.Set<CandidateLanguage>().RemoveRange(profile.Languages);
                profile.Languages.Clear();
            }

            foreach (var language in languages)
            {
                profile.Languages.Add(new CandidateLanguage
                {
                    CandidateProfileId = profile.Id,
                    LanguageCode = language.LanguageCode.Trim().ToLowerInvariant(),
                    Level = language.Level.Trim().ToLowerInvariant()
                });
            }
        }

        private void ReplaceExperiences(CandidateProfile profile,
            IReadOnlyCollection<CandidateExperienceRequest> experiences,
            bool hasNoExperience)
        {
            if (profile.Experiences.Count > 0)
            {
                _dbContext.Set<CandidateExperience>().RemoveRange(profile.Experiences);
                profile.Experiences.Clear();
            }

            if (hasNoExperience)
            {
                return;
            }

            foreach (var experience in experiences)
            {
                profile.Experiences.Add(new CandidateExperience
                {
                    CandidateProfileId = profile.Id,
                    CompanyName = experience.CompanyName.Trim(),
                    Position = experience.Position.Trim(),
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    Description = string.IsNullOrWhiteSpace(experience.Description) ? null : experience.Description.Trim()
                });
            }
        }

        private void ReplaceEducations(CandidateProfile profile, IReadOnlyCollection<CandidateEducationRequest> educations)
        {
            if (profile.Educations.Count > 0)
            {
                _dbContext.Set<CandidateEducation>().RemoveRange(profile.Educations);
                profile.Educations.Clear();
            }

            foreach (var education in educations)
            {
                profile.Educations.Add(new CandidateEducation
                {
                    CandidateProfileId = profile.Id,
                    InstitutionName = education.InstitutionName.Trim(),
                    Faculty = string.IsNullOrWhiteSpace(education.Faculty) ? null : education.Faculty.Trim(),
                    Degree = string.IsNullOrWhiteSpace(education.Degree) ? null : education.Degree.Trim(),
                    StartYear = education.StartYear,
                    EndYear = education.EndYear
                });
            }
        }

        private void ReplaceCertificates(CandidateProfile profile, IReadOnlyCollection<CandidateCertificateRequest> certificates)
        {
            if (profile.Certificates.Count > 0)
            {
                _dbContext.Set<CandidateCertificate>().RemoveRange(profile.Certificates);
                profile.Certificates.Clear();
            }

            foreach (var certificate in certificates)
            {
                profile.Certificates.Add(new CandidateCertificate
                {
                    CandidateProfileId = profile.Id,
                    Name = certificate.Name.Trim(),
                    Issuer = string.IsNullOrWhiteSpace(certificate.Issuer) ? null : certificate.Issuer.Trim(),
                    IssueDate = certificate.IssueDate,
                    ExpirationDate = certificate.ExpirationDate,
                    CredentialId = string.IsNullOrWhiteSpace(certificate.CredentialId) ? null : certificate.CredentialId.Trim(),
                    CredentialUrl = string.IsNullOrWhiteSpace(certificate.CredentialUrl) ? null : certificate.CredentialUrl.Trim()
                });
            }
        }

        private async Task ReplaceSkillsAsync(CandidateProfile profile,
            IReadOnlyCollection<CandidateSkillRequest> skills,
            CancellationToken cancellationToken)
        {
            if (profile.Skills.Count > 0)
            {
                _dbContext.Set<CandidateSkill>().RemoveRange(profile.Skills);
                profile.Skills.Clear();
            }

            var normalizedSkills = skills
                .Select(x => new CandidateSkillRequest(
                    x.SkillCode.Trim().ToLowerInvariant(),
                    string.IsNullOrWhiteSpace(x.Name) ? null : x.Name.Trim()))
                .GroupBy(x => x.SkillCode)
                .Select(x => x.First())
                .ToArray();

            foreach (var skill in normalizedSkills)
            {
                var exists = await _dbContext.Set<DictionaryItem>()
                    .AnyAsync(x => x.Type == DictionaryTypes.Skill && x.Code == skill.SkillCode && x.IsActive,
                        cancellationToken);
                if (!exists)
                {
                    throw new InvalidOperationException($"Skill '{skill.SkillCode}' is not active or does not exist.");
                }

                profile.Skills.Add(new CandidateSkill
                {
                    CandidateProfileId = profile.Id,
                    SkillCode = skill.SkillCode,
                    Name = skill.Name
                });
            }
        }

        private static bool IsProfileComplete(CandidateProfile profile)
            => !string.IsNullOrWhiteSpace(profile.FirstName)
               && !string.IsNullOrWhiteSpace(profile.LastName)
               && profile.DateOfBirth.HasValue
               && !string.IsNullOrWhiteSpace(profile.Citizenship)
               && !string.IsNullOrWhiteSpace(profile.CountryOfResidence)
               && !string.IsNullOrWhiteSpace(profile.DesiredPosition)
               && profile.Languages.Any(x => !x.IsDeleted)
               && profile.Skills.Any(x => !x.IsDeleted)
               && (profile.HasNoExperience || profile.Experiences.Any(x => !x.IsDeleted));

        private static CandidateProfileDto ToDto(CandidateProfile profile)
            => new(
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
                profile.PhotoFileId,
                profile.PhotoUrl,
                profile.IsVisible,
                profile.JobSearchStatus,
                profile.HasNoExperience,
                profile.IsComplete,
                profile.CompletedAt,
                profile.ModerationStatus,
                profile.ModerationComment,
                profile.ModeratedByUserId,
                profile.ModeratedAt,
                profile.Languages
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.LanguageCode)
                    .Select(x => new CandidateLanguageDto(x.Id, x.LanguageCode, x.Level))
                    .ToArray(),
                profile.Experiences
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new CandidateExperienceDto(x.Id, x.CompanyName, x.Position, x.StartDate, x.EndDate,
                        x.Description))
                    .ToArray(),
                profile.Educations
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.EndYear ?? x.StartYear ?? 0)
                    .Select(x => new CandidateEducationDto(x.Id, x.InstitutionName, x.Faculty, x.Degree, x.StartYear, x.EndYear))
                    .ToArray(),
                profile.Certificates
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.IssueDate)
                    .Select(x => new CandidateCertificateDto(x.Id, x.Name, x.Issuer, x.IssueDate, x.ExpirationDate,
                        x.CredentialId, x.CredentialUrl))
                    .ToArray(),
                profile.Skills
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SkillCode)
                    .Select(x => new CandidateSkillDto(x.Id, x.SkillCode, x.Name))
                    .ToArray());
    }
}
