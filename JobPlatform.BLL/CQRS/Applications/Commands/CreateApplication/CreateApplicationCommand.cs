using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.Common.Notifications;
using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.CreateApplication;

public sealed record CreateApplicationCommand(Guid VacancyId, Guid ResumeId, string? CoverLetter)
    : IRequest<ApplicationDto>
{
    public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, ApplicationDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;

        public CreateApplicationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService, IEmailTemplateRenderer emailTemplateRenderer, IEmailSender emailSender,
            INotificationService notificationService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
            _emailTemplateRenderer = emailTemplateRenderer;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        public async Task<ApplicationDto> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var candidate = await _db.Set<CandidateProfile>().FirstOrDefaultAsync(
                                x => x.UserId == userId && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Профиль кандидата не найден.");

            var resume = await _db.Set<Resume>().FirstOrDefaultAsync(
                             x => x.Id == request.ResumeId && x.CandidateProfileId == candidate.Id && !x.IsDeleted,
                             cancellationToken)
                         ?? throw new InvalidOperationException("Резюме не найдено или недоступно.");

            var vacancy = await _db.Set<JobVacancy>().FirstOrDefaultAsync(
                              x => x.Id == request.VacancyId && x.Status == "Published" && !x.IsDeleted,
                              cancellationToken)
                          ?? throw new InvalidOperationException("Опубликованная вакансия не найдена.");

            var exists = await _db.Set<JobApplication>().AnyAsync(
                x => x.VacancyId == request.VacancyId && x.CandidateProfileId == candidate.Id && !x.IsDeleted,
                cancellationToken);

            if (exists) throw new InvalidOperationException("Отклик на эту вакансию уже создан.");

            var application = new JobApplication
            {
                VacancyId = vacancy.Id,
                CandidateProfileId = candidate.Id,
                ResumeId = resume.Id,
                CoverLetter = request.CoverLetter,
                Status = ApplicationStatuses.New
            };
            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                JobApplication = application,
                OldStatus = null,
                NewStatus = ApplicationStatuses.New,
                ChangedByUserId = userId,
                Comment = "Отклик создан кандидатом."
            });

            await _db.Set<JobApplication>().AddAsync(application, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.ApplicationCreated,
                EntityType: nameof(JobApplication),
                EntityId: application.Id,
                NewValue: new
                    { application.VacancyId, application.CandidateProfileId, application.ResumeId, application.Status },
                UserId: userId), cancellationToken);

            var companyMembers = await _db.Set<CompanyMember>()
                .Include(x => x.User)
                .Where(x => x.CompanyId == vacancy.CompanyId && x.Status == "Active" && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var candidateName = $"{candidate.FirstName} {candidate.LastName}".Trim();
            var emailTemplate =
                _emailTemplateRenderer.RenderApplicationCreated(vacancy.Title, candidateName, request.CoverLetter);
            var notificationTitle = "Новый отклик на вакансию";
            var notificationMessage = $"Кандидат {candidateName} откликнулся на вакансию '{vacancy.Title}'.";
            await _notificationService.CreateInternalForUsersAsync(
                companyMembers.Select(x => x.UserId),
                NotificationTypes.ApplicationCreated,
                notificationTitle,
                notificationMessage,
                nameof(JobApplication),
                application.Id,
                cancellationToken);

            foreach (var member in companyMembers.Where(x => !string.IsNullOrWhiteSpace(x.User.Email)))
            {
                await _emailSender.SendAsync(member.User.Email, emailTemplate.Subject, emailTemplate.HtmlBody,
                    emailTemplate.TextBody, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new ApplicationDto(application.Id, vacancy.Id, vacancy.Title, candidate.Id, candidateName,
                null, null, false, resume.Id, application.Status, application.CoverLetter, application.CreatedAt);
        }
    }
}