using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.Common.Notifications;
using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.ChangeApplicationStatus;

public sealed record ChangeApplicationStatusCommand(Guid ApplicationId, string NewStatus, string? Comment)
    : IRequest<ApplicationDto>
{
    public class ChangeApplicationStatusCommandHandler : IRequestHandler<ChangeApplicationStatusCommand, ApplicationDto>
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Sent", "Viewed", "InProgress", "Interview", "Rejected", "Accepted", "Closed"
        };

        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;

        public ChangeApplicationStatusCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService, INotificationService notificationService, IEmailSender emailSender,
            IEmailTemplateRenderer emailTemplateRenderer)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _emailTemplateRenderer = emailTemplateRenderer;
        }

        public async Task<ApplicationDto> Handle(ChangeApplicationStatusCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            if (!AllowedStatuses.Contains(request.NewStatus))
                throw new InvalidOperationException("Недопустимый статус отклика.");

            var application = await _db.Set<JobApplication>()
                                  .Include(x => x.Vacancy)
                                  .Include(x => x.CandidateProfile)
                                  .FirstOrDefaultAsync(x => x.Id == request.ApplicationId && !x.IsDeleted,
                                      cancellationToken)
                              ?? throw new InvalidOperationException("Отклик не найден.");


            var hasAccess = await _db.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == application.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к откликам этой вакансии.");

            var oldStatus = application.Status;
            application.Status = request.NewStatus;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                JobApplicationId = application.Id,
                OldStatus = oldStatus,
                NewStatus = request.NewStatus,
                ChangedByUserId = userId,
                Comment = request.Comment
            });

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.ApplicationStatusChanged,
                EntityType: nameof(JobApplication),
                EntityId: application.Id,
                OldValue: new { Status = oldStatus },
                NewValue: new { Status = request.NewStatus, request.Comment },
                UserId: userId), cancellationToken);

            var emailTemplate = _emailTemplateRenderer.RenderApplicationStatusChanged(application.Vacancy.Title,
                oldStatus, request.NewStatus, request.Comment);
            var notificationTitle = "Статус отклика изменен";
            var notificationMessage =
                $"Статус отклика на вакансию '{application.Vacancy.Title}' изменен: {oldStatus} -> {request.NewStatus}.";
            await _notificationService.CreateInternalAsync(
                application.CandidateProfile.UserId,
                NotificationTypes.ApplicationStatusChanged,
                notificationTitle,
                notificationMessage,
                nameof(JobApplication),
                application.Id,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(application.CandidateProfile.User.Email))
            {
                await _emailSender.SendAsync(application.CandidateProfile.User.Email, emailTemplate.Subject,
                    emailTemplate.HtmlBody, emailTemplate.TextBody, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            var candidateName =
                $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName}".Trim();

            return new ApplicationDto(application.Id, application.VacancyId, application.Vacancy.Title,
                application.CandidateProfileId, candidateName, application.ResumeId, application.Status,
                application.CoverLetter, application.CreatedAt);
        }
    }
}