using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.Core.Entities.Files;
using JobPlatform.Core.Entities.Questionnaires;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Questionnaires.Commands.SubmitQuestionnaireResponse;

public sealed record SubmitQuestionnaireResponseCommand(
    Guid QuestionnaireId,
    string EntityType,
    Guid? EntityId,
    IReadOnlyCollection<SubmitQuestionnaireAnswerDto> Answers) : IRequest<QuestionnaireResponseDto>
{
    public sealed class
        SubmitQuestionnaireResponseCommandHandler : IRequestHandler<SubmitQuestionnaireResponseCommand,
        QuestionnaireResponseDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public SubmitQuestionnaireResponseCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<QuestionnaireResponseDto> Handle(SubmitQuestionnaireResponseCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var questionnaire = await _db.Set<Questionnaire>()
                                    .Include(x => x.Sections).ThenInclude(x => x.Questions).ThenInclude(x => x.Options)
                                    .FirstOrDefaultAsync(x => x.Id == request.QuestionnaireId, cancellationToken)
                                ?? throw new KeyNotFoundException("Анкета не найдена.");
            if (!questionnaire.IsActive) throw new InvalidOperationException("Анкета неактивна.");
            if (questionnaire.EntityType != request.EntityType || questionnaire.EntityId != request.EntityId)
                throw new InvalidOperationException("Ответ не соответствует привязке анкеты.");

            var answersByQuestionId =
                request.Answers.GroupBy(x => x.QuestionId).ToDictionary(x => x.Key, x => x.First());
            var questions = questionnaire.Sections.SelectMany(x => x.Questions).ToArray();
            foreach (var required in questions.Where(x => x.IsRequired))
            {
                if (!answersByQuestionId.TryGetValue(required.Id, out var answer) ||
                    (string.IsNullOrWhiteSpace(answer.Value) && !answer.FileId.HasValue))
                    throw new InvalidOperationException($"Обязательный вопрос не заполнен: {required.Text}");
            }

            foreach (var answer in request.Answers)
            {
                if (questions.All(x => x.Id != answer.QuestionId))
                    throw new InvalidOperationException("Ответ содержит вопрос, который не относится к анкете.");
                if (answer.FileId.HasValue && !await _db.Set<StoredFile>()
                        .AnyAsync(x => x.Id == answer.FileId.Value && x.UploadedByUserId == userId, cancellationToken))
                    throw new InvalidOperationException(
                        "Файл ответа не найден или не принадлежит текущему пользователю.");
            }

            var response = new QuestionnaireResponse
            {
                QuestionnaireId = questionnaire.Id,
                UserId = userId,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                SubmittedAt = DateTimeOffset.UtcNow
            };
            foreach (var answer in request.Answers)
            {
                response.Answers.Add(new QuestionnaireAnswer
                    { QuestionId = answer.QuestionId, Value = answer.Value, FileId = answer.FileId });
            }

            await _db.Set<QuestionnaireResponse>().AddAsync(response, cancellationToken);
            await _auditService.AddAsync(
                new AuditEvent(AuditActions.QuestionnaireResponseSubmitted, nameof(Questionnaire), questionnaire.Id,
                    NewValue: new { response.Id, response.EntityType, response.EntityId }, UserId: userId),
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var saved = await _db.Set<QuestionnaireResponse>()
                .Include(x => x.Answers).ThenInclude(x => x.Question)
                .FirstAsync(x => x.Id == response.Id, cancellationToken);
            return saved.ToDto();
        }
    }
}