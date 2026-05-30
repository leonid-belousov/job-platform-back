using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateOrUpdateProfile;

public sealed record CandidateLanguageRequest(string LanguageCode, string Level);

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
    bool IsVisible,
    IReadOnlyCollection<CandidateLanguageRequest> Languages) : IRequest<CandidateProfileDto>
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
                    profile.IsVisible,
                    profile.JobSearchStatus,
                    Languages = profile.Languages.Select(x => new { x.LanguageCode, x.Level }).ToArray()
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
            profile.IsVisible = request.IsVisible;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            if (profile.Languages.Count > 0)
            {
                _dbContext.Set<CandidateLanguage>().RemoveRange(profile.Languages);
            }

            foreach (var language in request.Languages)
            {
                profile.Languages.Add(new CandidateLanguage
                {
                    CandidateProfileId = profile.Id,
                    LanguageCode = language.LanguageCode.Trim().ToLowerInvariant(),
                    Level = language.Level.Trim().ToLowerInvariant()
                });
            }

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
                    profile.IsVisible,
                    profile.JobSearchStatus,
                    Languages = profile.Languages.Select(x => new { x.LanguageCode, x.Level }).ToArray()
                },
                UserId: userId), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ToDto(profile);
        }

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
                profile.IsVisible,
                profile.JobSearchStatus,
                profile.ModerationStatus,
                profile.ModerationComment,
                profile.ModeratedByUserId,
                profile.ModeratedAt,
                profile.Languages
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.LanguageCode)
                    .Select(x => new CandidateLanguageDto(x.Id, x.LanguageCode, x.Level))
                    .ToArray());
    }
}