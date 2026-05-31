using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Notifications;
using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.CreateInterviewInvitation;

public sealed record CreateInterviewInvitationCommand(
    Guid ApplicationId,
    DateTimeOffset ScheduledAt,
    string Format,
    string? Location,
    string? MeetingUrl,
    string? Message) : IRequest<InterviewInvitationDto>
{
    public sealed class Handler : IRequestHandler<CreateInterviewInvitationCommand, InterviewInvitationDto>
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

        public async Task<InterviewInvitationDto> Handle(CreateInterviewInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var application = await _db.Set<JobApplication>()
                                  .Include(x => x.Vacancy)
                                  .Include(x => x.CandidateProfile)
                                  .ThenInclude(x => x.User)
                                  .FirstOrDefaultAsync(x => x.Id == request.ApplicationId && !x.IsDeleted,
                                      cancellationToken)
                              ?? throw new InvalidOperationException("Application not found.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == application.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException();
            }

            var invitation = new InterviewInvitation
            {
                ApplicationId = application.Id,
                ScheduledAt = request.ScheduledAt,
                Format = request.Format.Trim().ToLowerInvariant(),
                Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
                MeetingUrl = string.IsNullOrWhiteSpace(request.MeetingUrl) ? null : request.MeetingUrl.Trim(),
                Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
                Status = InterviewInvitationStatuses.Pending,
                CreatedByUserId = userId
            };

            var oldStatus = application.Status;
            application.Status = ApplicationStatuses.Interview;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                JobApplicationId = application.Id,
                OldStatus = oldStatus,
                NewStatus = ApplicationStatuses.Interview,
                ChangedByUserId = userId,
                Comment = "Interview invitation created."
            });

            await _db.Set<InterviewInvitation>().AddAsync(invitation, cancellationToken);

            var notificationTitle = "Приглашение на интервью";
            var notificationMessage = $"Вас пригласили на интервью по вакансии '{application.Vacancy.Title}'.";
            await _notificationService.CreateInternalAsync(
                application.CandidateProfile.UserId,
                NotificationTypes.InterviewInvitationCreated,
                notificationTitle,
                notificationMessage,
                nameof(InterviewInvitation),
                invitation.Id,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(application.CandidateProfile.User.Email))
            {
                var emailTemplate = _emailTemplateRenderer.RenderInterviewInvitationCreated(
                    application.Vacancy.Title,
                    invitation.ScheduledAt,
                    invitation.Format,
                    invitation.Location,
                    invitation.MeetingUrl,
                    invitation.Message);
                await _emailSender.SendAsync(application.CandidateProfile.User.Email, emailTemplate.Subject,
                    emailTemplate.HtmlBody, emailTemplate.TextBody, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return ToDto(invitation);
        }

        private static InterviewInvitationDto ToDto(InterviewInvitation x)
            => new(x.Id, x.ApplicationId, x.ScheduledAt, x.Format, x.Location, x.MeetingUrl, x.Message,
                x.Status, x.CreatedByUserId, x.CandidateRespondedAt, x.CreatedAt);
    }
}