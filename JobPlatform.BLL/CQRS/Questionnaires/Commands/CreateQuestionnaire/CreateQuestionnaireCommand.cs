using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.CRM;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.CreateQuestionnaire;

public sealed record CreateQuestionnaireCommand(
    string Title,
    string? Description,
    string EntityType,
    Guid? EntityId,
    bool IsActive) : IRequest<QuestionnaireDto>
{
    public sealed class
        CreateQuestionnaireCommandHandler : IRequestHandler<CreateQuestionnaireCommand, QuestionnaireDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public CreateQuestionnaireCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<QuestionnaireDto> Handle(CreateQuestionnaireCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            await EnsureEntityExistsAsync(request.EntityType, request.EntityId, cancellationToken);

            var questionnaire = new Questionnaire
            {
                Title = request.Title.Trim(),
                Description = request.Description,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                IsActive = request.IsActive,
                CreatedByUserId = userId
            };

            await _db.Set<Questionnaire>().AddAsync(questionnaire, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.QuestionnaireCreated, nameof(Questionnaire), questionnaire.Id,
                    NewValue: new { questionnaire.Title, questionnaire.EntityType, questionnaire.EntityId },
                    UserId: userId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return questionnaire.ToDto();
        }

        private async Task EnsureEntityExistsAsync(string entityType, Guid? entityId,
            CancellationToken cancellationToken)
        {
            if (!entityId.HasValue) return;
            var exists = entityType switch
            {
                QuestionnaireEntityTypes.Candidate => await _db.Set<CandidateProfile>()
                    .AnyAsync(x => x.Id == entityId.Value, cancellationToken),
                QuestionnaireEntityTypes.Vacancy => await _db.Set<JobVacancy>()
                    .AnyAsync(x => x.Id == entityId.Value, cancellationToken),
                QuestionnaireEntityTypes.Company => await _db.Set<Company>()
                    .AnyAsync(x => x.Id == entityId.Value, cancellationToken),
                QuestionnaireEntityTypes.Application => await _db.Set<JobApplication>()
                    .AnyAsync(x => x.Id == entityId.Value, cancellationToken),
                QuestionnaireEntityTypes.CrmLead => await _db.Set<CrmLead>()
                    .AnyAsync(x => x.Id == entityId.Value, cancellationToken),
                _ => false
            };
            if (!exists) throw new KeyNotFoundException("Сущность для привязки анкеты не найдена.");
        }
    }
}