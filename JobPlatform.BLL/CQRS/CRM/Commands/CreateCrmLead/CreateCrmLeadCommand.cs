using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.CRM.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.Core.Entities.Users;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.CRM.Commands.CreateCrmLead;

public sealed record CreateCrmLeadCommand(
    string Type,
    string Name,
    string? Source,
    string? Description,
    Guid? ResponsibleUserId,
    Guid? CandidateProfileId,
    Guid? CompanyId,
    Guid? VacancyId,
    Guid? ApplicationId) : IRequest<CrmLeadDto>
{
    public class CreateCrmLeadCommandHandler : IRequestHandler<CreateCrmLeadCommand, CrmLeadDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public CreateCrmLeadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CrmLeadDto> Handle(CreateCrmLeadCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            if (request.ResponsibleUserId.HasValue && !await _db.Set<User>()
                    .AnyAsync(x => x.Id == request.ResponsibleUserId.Value, cancellationToken))
                throw new KeyNotFoundException("Ответственный пользователь не найден.");
            if (request.CandidateProfileId.HasValue && !await _db.Set<CandidateProfile>()
                    .AnyAsync(x => x.Id == request.CandidateProfileId.Value, cancellationToken))
                throw new KeyNotFoundException("Профиль кандидата не найден.");
            if (request.CompanyId.HasValue &&
                !await _db.Set<Company>().AnyAsync(x => x.Id == request.CompanyId.Value, cancellationToken))
                throw new KeyNotFoundException("Компания не найдена.");
            if (request.VacancyId.HasValue &&
                !await _db.Set<JobVacancy>().AnyAsync(x => x.Id == request.VacancyId.Value, cancellationToken))
                throw new KeyNotFoundException("Вакансия не найдена.");
            if (request.ApplicationId.HasValue && !await _db.Set<JobApplication>()
                    .AnyAsync(x => x.Id == request.ApplicationId.Value, cancellationToken))
                throw new KeyNotFoundException("Отклик не найден.");

            var lead = new CrmLead
            {
                Type = request.Type,
                Name = request.Name.Trim(),
                Source = request.Source,
                Description = request.Description,
                ResponsibleUserId = request.ResponsibleUserId,
                CandidateProfileId = request.CandidateProfileId,
                CompanyId = request.CompanyId,
                VacancyId = request.VacancyId,
                ApplicationId = request.ApplicationId,
                Status = CrmLeadStatuses.New
            };

            lead.Activities.Add(new CrmActivity
            {
                Type = CrmActivityTypes.Note,
                Description = "CRM-лид создан.",
                CreatedByUserId = userId
            });

            await _db.Set<CrmLead>().AddAsync(lead, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.CrmLeadCreated, nameof(CrmLead), lead.Id,
                    NewValue: new { lead.Type, lead.Name, lead.Status }, UserId: userId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return lead.ToDto();
        }
    }
}