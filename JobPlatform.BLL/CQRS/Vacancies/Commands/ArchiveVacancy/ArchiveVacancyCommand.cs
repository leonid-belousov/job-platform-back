using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.ArchiveVacancy;

public sealed record ArchiveVacancyCommand(Guid VacancyId) : IRequest<VacancyDto>
{
    public class ArchiveVacancyCommandHandler : IRequestHandler<ArchiveVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public ArchiveVacancyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<VacancyDto> Handle(ArchiveVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            
            var vacancy = await _db.Set<JobVacancy>()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Вакансия не найдена.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к вакансии.");
            
            var oldStatus = vacancy.Status;
            
            vacancy.Status = "Archived";
            vacancy.ArchivedAt = DateTimeOffset.UtcNow;
            vacancy.UpdatedAt = DateTimeOffset.UtcNow;
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.VacancyArchived,
                EntityType: nameof(JobVacancy),
                EntityId: vacancy.Id,
                OldValue: new { Status = oldStatus },
                NewValue: new { vacancy.Status, vacancy.ArchivedAt },
                UserId: userId), cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);
            
            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat, vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status);
        }
    }
}