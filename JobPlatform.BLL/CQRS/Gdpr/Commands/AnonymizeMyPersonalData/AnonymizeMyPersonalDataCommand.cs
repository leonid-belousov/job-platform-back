using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Legal;
using JobPlatform.Core.Entities.Notifications;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Gdpr.Commands.AnonymizeMyPersonalData;

public sealed record AnonymizeMyPersonalDataCommand(bool Confirm) : IRequest
{
    public sealed class Handler : IRequestHandler<AnonymizeMyPersonalDataCommand>
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

        public async Task Handle(AnonymizeMyPersonalDataCommand request, CancellationToken cancellationToken)
        {
            if (!request.Confirm)
            {
                throw new InvalidOperationException("Personal data anonymization must be explicitly confirmed.");
            }

            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var user = await _db.Set<User>()
                           .Include(x => x.RefreshTokens)
                           .Include(x => x.AuthTokens)
                           .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("User not found.");

            var now = DateTimeOffset.UtcNow;
            var anonymizedEmail = $"deleted-{user.Id:N}@deleted.local";
            var oldValue = new { user.Email, user.Phone, user.FirstName, user.LastName, user.Status };

            user.Email = anonymizedEmail;
            user.Phone = null;
            user.FirstName = "Deleted";
            user.LastName = "User";
            user.PasswordHash = string.Empty;
            user.EmailConfirmed = false;
            user.PhoneConfirmed = false;
            user.Status = UserStatus.Blocked;
            user.IsDeleted = true;
            user.DeletedAt = now;
            user.UpdatedAt = now;

            foreach (var refreshToken in user.RefreshTokens.Where(x => x.RevokedAt == null))
            {
                refreshToken.RevokedAt = now;
                refreshToken.ReplacedByTokenHash = null;
            }

            foreach (var authToken in user.AuthTokens.Where(x => x.UsedAt == null && !x.IsDeleted))
            {
                authToken.UsedAt = now;
                authToken.IsDeleted = true;
                authToken.DeletedAt = now;
                authToken.UpdatedAt = now;
            }

            var profiles = await _db.Set<CandidateProfile>()
                .Include(x => x.Resumes.Where(resume => !resume.IsDeleted))
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToArrayAsync(cancellationToken);

            foreach (var profile in profiles)
            {
                profile.FirstName = "Deleted";
                profile.MiddleName = null;
                profile.LastName = "Candidate";
                profile.DateOfBirth = null;
                profile.Citizenship = null;
                profile.CountryOfResidence = null;
                profile.City = null;
                profile.Phone = null;
                profile.DesiredPosition = null;
                profile.ExpectedSalary = null;
                profile.Currency = null;
                profile.About = null;
                profile.IsVisible = false;
                profile.IsComplete = false;
                profile.CompletedAt = null;
                profile.ModerationComment = null;
                profile.IsDeleted = true;
                profile.DeletedAt = now;
                profile.UpdatedAt = now;

                foreach (var resume in profile.Resumes)
                {
                    resume.Title = "Deleted resume";
                    resume.FileId = null;
                    resume.Status = "Deleted";
                    resume.IsActive = false;
                    resume.IsDefault = false;
                    resume.IsDeleted = true;
                    resume.DeletedAt = now;
                    resume.UpdatedAt = now;
                }
            }

            var notifications = await _db.Set<Notification>()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToArrayAsync(cancellationToken);
            foreach (var notification in notifications)
            {
                notification.IsDeleted = true;
                notification.DeletedAt = now;
                notification.UpdatedAt = now;
            }

            var legalConsents = await _db.Set<UserLegalConsent>()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToArrayAsync(cancellationToken);
            foreach (var consent in legalConsents)
            {
                consent.IpAddress = null;
                consent.UpdatedAt = now;
            }

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.PersonalDataAnonymized,
                EntityType: nameof(User),
                EntityId: user.Id,
                OldValue: oldValue,
                NewValue: new { user.Email, user.IsDeleted, user.DeletedAt },
                UserId: user.Id), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
