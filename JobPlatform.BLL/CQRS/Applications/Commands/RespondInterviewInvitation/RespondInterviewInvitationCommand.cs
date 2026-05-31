using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Notifications;
using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.RespondInterviewInvitation;

public sealed record RespondInterviewInvitationCommand(Guid InvitationId, string ResponseStatus)
    : IRequest<InterviewInvitationDto>
{
    public sealed class Handler : IRequestHandler<RespondInterviewInvitationCommand, InterviewInvitationDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser,
            INotificationService notificationService, IEmailSender emailSender,
            IEmailTemplateRenderer emailTemplateRenderer)
        {
            _db = db;
            _currentUser = currentUser;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _emailTemplateRenderer = emailTemplateRenderer;
        }

        public async Task<InterviewInvitationDto> Handle(RespondInterviewInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var responseStatus = request.ResponseStatus.Trim().ToLowerInvariant();

            if (responseStatus != InterviewInvitationStatuses.Accepted && responseStatus != InterviewInvitationStatuses.Declined)
            {
                throw new InvalidOperationException("Invalid interview response.");
            }

            var invitation = await _db.Set<InterviewInvitation>()
                                 .Include(x => x.Application)
                                 .ThenInclude(x => x.Vacancy)
                                 .Include(x => x.Application)
                                 .ThenInclude(x => x.CandidateProfile)
                                 .FirstOrDefaultAsync(x => x.Id == request.InvitationId && !x.IsDeleted,
                                     cancellationToken)
                             ?? throw new InvalidOperationException("Interview invitation not found.");

            var candidate = await _db.Set<CandidateProfile>().AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == invitation.Application.CandidateProfileId && x.UserId == userId && !x.IsDeleted,
                                    cancellationToken)
                            ?? throw new UnauthorizedAccessException();

            invitation.Status = responseStatus;
            invitation.CandidateRespondedAt = DateTimeOffset.UtcNow;
            invitation.UpdatedAt = DateTimeOffset.UtcNow;

            var companyMembers = await _db.Set<CompanyMember>()
                .Include(x => x.User)
                .Where(x => x.CompanyId == invitation.Application.Vacancy.CompanyId && x.Status == "Active" && !x.IsDeleted)
                .Select(x => x.User)
                .ToListAsync(cancellationToken);

            var assignedRecruiters = await _db.Set<VacancyRecruiter>()
                .Include(x => x.RecruiterUser)
                .Where(x => x.VacancyId == invitation.Application.VacancyId && x.Status == "active" && !x.IsDeleted)
                .Select(x => x.RecruiterUser)
                .ToListAsync(cancellationToken);

            var recipients = companyMembers
                .Concat(assignedRecruiters)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToArray();

            var candidateName = $"{candidate.FirstName} {candidate.LastName}".Trim();
            var notificationTitle = "Ответ на приглашение на интервью";
            var notificationMessage = $"Кандидат {candidateName} ответил на приглашение по вакансии '{invitation.Application.Vacancy.Title}': {responseStatus}.";
            await _notificationService.CreateInternalForUsersAsync(
                recipients.Select(x => x.Id),
                NotificationTypes.InterviewInvitationResponded,
                notificationTitle,
                notificationMessage,
                nameof(InterviewInvitation),
                invitation.Id,
                cancellationToken);

            var emailTemplate = _emailTemplateRenderer.RenderInterviewInvitationResponded(
                invitation.Application.Vacancy.Title,
                candidateName,
                responseStatus,
                invitation.ScheduledAt);
            foreach (var recipient in recipients.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
            {
                await _emailSender.SendAsync(recipient.Email, emailTemplate.Subject, emailTemplate.HtmlBody,
                    emailTemplate.TextBody, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new InterviewInvitationDto(invitation.Id, invitation.ApplicationId, invitation.ScheduledAt,
                invitation.Format, invitation.Location, invitation.MeetingUrl, invitation.Message, invitation.Status,
                invitation.CreatedByUserId, invitation.CandidateRespondedAt, invitation.CreatedAt);
        }
    }
}