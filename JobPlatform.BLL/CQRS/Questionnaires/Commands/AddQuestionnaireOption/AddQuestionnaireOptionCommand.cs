using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireOption;

public sealed record AddQuestionnaireOptionCommand(Guid QuestionId, string Text, string Value, int SortOrder)
    : IRequest<QuestionnaireOptionDto>
{
    public sealed class
        AddQuestionnaireOptionCommandHandler : IRequestHandler<AddQuestionnaireOptionCommand, QuestionnaireOptionDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public AddQuestionnaireOptionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<QuestionnaireOptionDto> Handle(AddQuestionnaireOptionCommand request,
            CancellationToken cancellationToken)
        {
            var question = await _db.Set<QuestionnaireQuestion>().Include(x => x.Section)
                               .FirstOrDefaultAsync(x => x.Id == request.QuestionId, cancellationToken)
                           ?? throw new KeyNotFoundException("Вопрос анкеты не найден.");
            if (!QuestionnaireQuestionTypes.OptionBased.Contains(question.QuestionType))
                throw new InvalidOperationException("Варианты ответов доступны только для вопросов с выбором ответа.");

            var option = new QuestionnaireOption
            {
                QuestionId = request.QuestionId, Text = request.Text.Trim(), Value = request.Value.Trim(),
                SortOrder = request.SortOrder
            };
            await _db.Set<QuestionnaireOption>().AddAsync(option, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.QuestionnaireOptionAdded, nameof(Questionnaire),
                    question.Section.QuestionnaireId, NewValue: new { option.Id, option.Text, option.Value },
                    UserId: _currentUser.UserId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return option.ToDto();
        }
    }
}