using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireQuestion;

public sealed record AddQuestionnaireQuestionCommand(
    Guid SectionId,
    string Text,
    string QuestionType,
    bool IsRequired,
    string? Placeholder,
    string? HelpText,
    string? ValidationRules,
    string? VisibilityCondition,
    int SortOrder) : IRequest<QuestionnaireQuestionDto>
{
    public sealed class
        AddQuestionnaireQuestionCommandHandler : IRequestHandler<AddQuestionnaireQuestionCommand,
        QuestionnaireQuestionDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public AddQuestionnaireQuestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<QuestionnaireQuestionDto> Handle(AddQuestionnaireQuestionCommand request,
            CancellationToken cancellationToken)
        {
            var section = await _db.Set<QuestionnaireSection>().Include(x => x.Questionnaire)
                              .FirstOrDefaultAsync(x => x.Id == request.SectionId, cancellationToken)
                          ?? throw new KeyNotFoundException("Секция анкеты не найдена.");

            var question = new QuestionnaireQuestion
            {
                SectionId = request.SectionId,
                Text = request.Text.Trim(),
                QuestionType = request.QuestionType,
                IsRequired = request.IsRequired,
                Placeholder = request.Placeholder,
                HelpText = request.HelpText,
                ValidationRules = request.ValidationRules,
                VisibilityCondition = request.VisibilityCondition,
                SortOrder = request.SortOrder
            };
            await _db.Set<QuestionnaireQuestion>().AddAsync(question, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.QuestionnaireQuestionAdded, nameof(Questionnaire), section.QuestionnaireId,
                    NewValue: new { question.Id, question.Text, question.QuestionType }, UserId: _currentUser.UserId),
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return question.ToDto();
        }
    }
}