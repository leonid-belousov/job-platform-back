using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.PublishVacancy;

public sealed record PublishVacancyCommand(Guid VacancyId) : IRequest<VacancyDto>
{
    public class PublishVacancyCommandHandler : IRequestHandler<PublishVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public PublishVacancyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<VacancyDto> Handle(PublishVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var vacancy = await _db.Set<JobVacancy>()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Вакансия не найдена.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к вакансии.");

            if (string.IsNullOrWhiteSpace(vacancy.Title) || string.IsNullOrWhiteSpace(vacancy.Description))
                throw new InvalidOperationException("Для публикации вакансии обязательны название и описание.");

            var oldStatus = vacancy.Status;
            var oldModerationStatus = vacancy.ModerationStatus;

            if (vacancy.ModerationStatus == ModerationStatuses.Approved)
            {
                var now = DateTimeOffset.UtcNow;
                vacancy.Status = "Published";
                vacancy.PublishedAt ??= now;
                vacancy.ExpiresAt ??= now.AddDays(30);
                await _auditService.AddAsync(new AuditEvent(
                    AuditActions.VacancyPublished,
                    EntityType: nameof(JobVacancy),
                    EntityId: vacancy.Id,
                    OldValue: new { Status = oldStatus, ModerationStatus = oldModerationStatus },
                    NewValue: new { vacancy.Status, vacancy.ModerationStatus, vacancy.PublishedAt, vacancy.ExpiresAt },
                    UserId: userId), cancellationToken);
            }
            else
            {
                vacancy.Status = "PendingModeration";
                vacancy.ModerationStatus = ModerationStatuses.Pending;
                vacancy.ModerationComment = null;
                vacancy.ModeratedAt = null;
                vacancy.ModeratedByUserId = null;
                await _auditService.AddAsync(new AuditEvent(
                    AuditActions.VacancySubmittedForModeration,
                    EntityType: nameof(JobVacancy),
                    EntityId: vacancy.Id,
                    OldValue: new { Status = oldStatus, ModerationStatus = oldModerationStatus },
                    NewValue: new { vacancy.Status, vacancy.ModerationStatus },
                    UserId: userId), cancellationToken);
            }

            vacancy.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat,
                vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status,
                vacancy.ModerationStatus, vacancy.ModerationComment, vacancy.ModeratedByUserId, vacancy.ModeratedAt,
                vacancy.PublishedAt, vacancy.ExpiresAt, vacancy.ExtendedAt, vacancy.ExtensionCount);
        }
    }
}