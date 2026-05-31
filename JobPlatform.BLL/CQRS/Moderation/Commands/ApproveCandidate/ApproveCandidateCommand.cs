using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.ApproveCandidate;

public sealed record ApproveCandidateCommand(Guid CandidateProfileId, string? Comment) : IRequest<CandidateProfileDto>
{
    public sealed class Handler : IRequestHandler<ApproveCandidateCommand, CandidateProfileDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CandidateProfileDto> Handle(ApproveCandidateCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var profile = await _db.Set<CandidateProfile>()
                              .Include(x => x.Languages.Where(language => !language.IsDeleted))
                              .Include(x => x.Experiences.Where(experience => !experience.IsDeleted))
                              .Include(x => x.Skills.Where(skill => !skill.IsDeleted))
                              .FirstOrDefaultAsync(x => x.Id == request.CandidateProfileId && !x.IsDeleted,
                                  cancellationToken)
                          ?? throw new KeyNotFoundException("Профиль кандидата не найден.");

            var oldValue = new { profile.ModerationStatus, profile.ModerationComment, profile.ModeratedByUserId, profile.ModeratedAt };
            profile.ModerationStatus = ModerationStatuses.Approved;
            profile.ModerationComment = request.Comment;
            profile.ModeratedByUserId = userId;
            profile.ModeratedAt = DateTimeOffset.UtcNow;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.CandidateApproved,
                EntityType: nameof(CandidateProfile),
                EntityId: profile.Id,
                OldValue: oldValue,
                NewValue: new { profile.ModerationStatus, profile.ModerationComment, profile.ModeratedByUserId, profile.ModeratedAt },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
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
                    .Select(x => new CandidateExperienceDto(x.Id, x.CompanyName, x.Position, x.StartDate, x.EndDate, x.Description))
                    .ToArray(),
                profile.Skills
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SkillCode)
                    .Select(x => new CandidateSkillDto(x.Id, x.SkillCode, x.Name))
                    .ToArray());
    }
}
