using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.UpdateQuestionnaire;

public sealed record UpdateQuestionnaireCommand(Guid Id, string Title, string? Description, bool IsActive)
    : IRequest<QuestionnaireDto>
{
    public sealed class
        UpdateQuestionnaireCommandHandler : IRequestHandler<UpdateQuestionnaireCommand, QuestionnaireDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public UpdateQuestionnaireCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<QuestionnaireDto> Handle(UpdateQuestionnaireCommand request,
            CancellationToken cancellationToken)
        {
            var questionnaire = await _db.Set<Questionnaire>().Include(x => x.Sections).ThenInclude(x => x.Questions)
                                    .ThenInclude(x => x.Options)
                                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                                ?? throw new KeyNotFoundException("Анкета не найдена.");
            var oldValue = new { questionnaire.Title, questionnaire.Description, questionnaire.IsActive };
            questionnaire.Title = request.Title.Trim();
            questionnaire.Description = request.Description;
            questionnaire.IsActive = request.IsActive;
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.QuestionnaireUpdated, nameof(Questionnaire), questionnaire.Id,
                    OldValue: oldValue,
                    NewValue: new { questionnaire.Title, questionnaire.Description, questionnaire.IsActive },
                    UserId: _currentUser.UserId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return questionnaire.ToDto();
        }
    }
}