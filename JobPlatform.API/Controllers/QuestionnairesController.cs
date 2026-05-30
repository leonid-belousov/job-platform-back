using JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireOption;
using JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireQuestion;
using JobPlatform.BLL.CQRS.Questionnaires.Commands.AddQuestionnaireSection;
using JobPlatform.BLL.CQRS.Questionnaires.Commands.CreateQuestionnaire;
using JobPlatform.BLL.CQRS.Questionnaires.Commands.SubmitQuestionnaireResponse;
using JobPlatform.BLL.CQRS.Questionnaires.Commands.UpdateQuestionnaire;
using JobPlatform.BLL.CQRS.Questionnaires.DTO;
using JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaireById;
using JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaireResponses;
using JobPlatform.BLL.CQRS.Questionnaires.Queries.GetQuestionnaires;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

public sealed record CreateQuestionnaireRequest(
    string Title,
    string? Description,
    string EntityType,
    Guid? EntityId,
    bool IsActive);

public sealed record UpdateQuestionnaireRequest(string Title, string? Description, bool IsActive);

public sealed record AddQuestionnaireSectionRequest(string Title, string? Description, int SortOrder);

public sealed record AddQuestionnaireQuestionRequest(
    string Text,
    string QuestionType,
    bool IsRequired,
    string? Placeholder,
    string? HelpText,
    string? ValidationRules,
    string? VisibilityCondition,
    int SortOrder);

public sealed record AddQuestionnaireOptionRequest(string Text, string Value, int SortOrder);

public sealed record SubmitQuestionnaireResponseRequest(
    string EntityType,
    Guid? EntityId,
    IReadOnlyCollection<SubmitQuestionnaireAnswerDto> Answers);

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "QuestionnairesRead")]
public sealed class QuestionnairesController : ControllerBase
{
    private readonly IMediator _mediator;
    public QuestionnairesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetQuestionnairesQuery(entityType, entityId, isActive, search, page, pageSize),
            cancellationToken));

    [HttpGet("{questionnaireId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid questionnaireId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetQuestionnaireByIdQuery(questionnaireId), cancellationToken));

    [HttpPost]
    [Authorize(Policy = "QuestionnairesManage")]
    public async Task<IActionResult> Create([FromBody] CreateQuestionnaireRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new CreateQuestionnaireCommand(request.Title, request.Description, request.EntityType, request.EntityId,
                request.IsActive), cancellationToken));

    [HttpPut("{questionnaireId:guid}")]
    [Authorize(Policy = "QuestionnairesManage")]
    public async Task<IActionResult> Update(Guid questionnaireId, [FromBody] UpdateQuestionnaireRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new UpdateQuestionnaireCommand(questionnaireId, request.Title, request.Description, request.IsActive),
            cancellationToken));

    [HttpPost("{questionnaireId:guid}/sections")]
    [Authorize(Policy = "QuestionnairesManage")]
    public async Task<IActionResult> AddSection(Guid questionnaireId, [FromBody] AddQuestionnaireSectionRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new AddQuestionnaireSectionCommand(questionnaireId, request.Title, request.Description, request.SortOrder),
            cancellationToken));

    [HttpPost("sections/{sectionId:guid}/questions")]
    [Authorize(Policy = "QuestionnairesManage")]
    public async Task<IActionResult> AddQuestion(Guid sectionId, [FromBody] AddQuestionnaireQuestionRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new AddQuestionnaireQuestionCommand(sectionId, request.Text, request.QuestionType, request.IsRequired,
                request.Placeholder, request.HelpText, request.ValidationRules, request.VisibilityCondition,
                request.SortOrder), cancellationToken));

    [HttpPost("questions/{questionId:guid}/options")]
    [Authorize(Policy = "QuestionnairesManage")]
    public async Task<IActionResult> AddOption(Guid questionId, [FromBody] AddQuestionnaireOptionRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new AddQuestionnaireOptionCommand(questionId, request.Text, request.Value, request.SortOrder),
            cancellationToken));

    [HttpPost("{questionnaireId:guid}/responses")]
    [Authorize]
    public async Task<IActionResult> SubmitResponse(Guid questionnaireId,
        [FromBody] SubmitQuestionnaireResponseRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new SubmitQuestionnaireResponseCommand(questionnaireId, request.EntityType, request.EntityId,
                request.Answers), cancellationToken));

    [HttpGet("{questionnaireId:guid}/responses")]
    [Authorize(Policy = "QuestionnairesManage")]
    public async Task<IActionResult> Responses(Guid questionnaireId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetQuestionnaireResponsesQuery(questionnaireId, page, pageSize),
            cancellationToken));
}