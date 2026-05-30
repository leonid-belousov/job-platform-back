using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireSection;

public sealed record AddQuestionnaireSectionCommand(
    Guid QuestionnaireId,
    string Title,
    string? Description,
    int SortOrder) : IRequest<QuestionnaireSectionDto>
{
    public sealed class
        AddQuestionnaireSectionCommandHandler : IRequestHandler<AddQuestionnaireSectionCommand, QuestionnaireSectionDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public AddQuestionnaireSectionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<QuestionnaireSectionDto> Handle(AddQuestionnaireSectionCommand request,
            CancellationToken cancellationToken)
        {
            if (!await _db.Set<Questionnaire>().AnyAsync(x => x.Id == request.QuestionnaireId, cancellationToken))
                throw new KeyNotFoundException("Анкета не найдена.");

            var section = new QuestionnaireSection
            {
                QuestionnaireId = request.QuestionnaireId, Title = request.Title.Trim(),
                Description = request.Description, SortOrder = request.SortOrder
            };
            await _db.Set<QuestionnaireSection>().AddAsync(section, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.QuestionnaireSectionAdded, nameof(Questionnaire), request.QuestionnaireId,
                    NewValue: new { section.Id, section.Title }, UserId: _currentUser.UserId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return section.ToDto();
        }
    }
}