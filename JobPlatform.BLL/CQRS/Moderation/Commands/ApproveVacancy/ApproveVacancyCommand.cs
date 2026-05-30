using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.ApproveVacancy;

public sealed record ApproveVacancyCommand(Guid VacancyId, string? Comment) : IRequest<VacancyDto>
{
    public class ApproveVacancyCommandHandler : IRequestHandler<ApproveVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public ApproveVacancyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<VacancyDto> Handle(ApproveVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var vacancy = await _db.Set<JobVacancy>()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new KeyNotFoundException("Вакансия не найдена.");

            var oldValue = new
                { vacancy.Status, vacancy.ModerationStatus, vacancy.ModerationComment, vacancy.PublishedAt };
            vacancy.ModerationStatus = ModerationStatuses.Approved;
            vacancy.ModerationComment = request.Comment;
            vacancy.ModeratedByUserId = userId;
            vacancy.ModeratedAt = DateTimeOffset.UtcNow;
            vacancy.UpdatedAt = DateTimeOffset.UtcNow;

            if (vacancy.Status == "PendingModeration")
            {
                vacancy.Status = "Published";
                vacancy.PublishedAt ??= DateTimeOffset.UtcNow;
            }

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.VacancyApproved,
                EntityType: nameof(JobVacancy),
                EntityId: vacancy.Id,
                OldValue: oldValue,
                NewValue: new
                    { vacancy.Status, vacancy.ModerationStatus, vacancy.ModerationComment, vacancy.PublishedAt },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat,
                vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status,
                vacancy.ModerationStatus, vacancy.ModerationComment, vacancy.ModeratedByUserId, vacancy.ModeratedAt);
        }
    }
}