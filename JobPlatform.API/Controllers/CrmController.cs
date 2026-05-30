using JobPlatform.BLL.CQRS.CRM.Commands.AddCrmActivity;
using JobPlatform.BLL.CQRS.CRM.Commands.CompleteCrmTask;
using JobPlatform.BLL.CQRS.CRM.Commands.CreateCrmLead;
using JobPlatform.BLL.CQRS.CRM.Commands.CreateCrmTask;
using JobPlatform.BLL.CQRS.CRM.Commands.UpdateCrmLead;
using JobPlatform.BLL.CQRS.CRM.Queries.GetCrmLeadById;
using JobPlatform.BLL.CQRS.CRM.Queries.GetCrmLeads;
using JobPlatform.BLL.CQRS.CRM.Queries.GetCrmTasks;
using JobPlatform.BLL.CQRS.CRM.Queries.GetCrmTimeline;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

public sealed record CreateCrmLeadRequest(
    string Type,
    string Name,
    string? Source,
    string? Description,
    Guid? ResponsibleUserId,
    Guid? CandidateProfileId,
    Guid? CompanyId,
    Guid? VacancyId,
    Guid? ApplicationId);

public sealed record UpdateCrmLeadRequest(
    string Name,
    string Status,
    string? Source,
    string? Description,
    Guid? ResponsibleUserId);

public sealed record CreateCrmTaskRequest(
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    Guid? ResponsibleUserId);

public sealed record AddCrmActivityRequest(
    string Type,
    string Description,
    Guid? RelatedEntityId,
    string? RelatedEntityType);

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CrmRead")]
public sealed class CrmController : ControllerBase
{
    private readonly IMediator _mediator;
    public CrmController(IMediator mediator) => _mediator = mediator;

    [HttpGet("leads")]
    public async Task<IActionResult> Leads(
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] Guid? responsibleUserId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetCrmLeadsQuery(type, status, responsibleUserId, search, page, pageSize),
            cancellationToken));

    [HttpGet("leads/{leadId:guid}")]
    public async Task<IActionResult> Lead(Guid leadId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCrmLeadByIdQuery(leadId), cancellationToken));

    [HttpPost("leads")]
    [Authorize(Policy = "CrmManage")]
    public async Task<IActionResult> CreateLead([FromBody] CreateCrmLeadRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new CreateCrmLeadCommand(request.Type, request.Name, request.Source, request.Description,
                request.ResponsibleUserId, request.CandidateProfileId, request.CompanyId, request.VacancyId,
                request.ApplicationId), cancellationToken));

    [HttpPut("leads/{leadId:guid}")]
    [Authorize(Policy = "CrmManage")]
    public async Task<IActionResult> UpdateLead(Guid leadId, [FromBody] UpdateCrmLeadRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new UpdateCrmLeadCommand(leadId, request.Name, request.Status, request.Source, request.Description,
                request.ResponsibleUserId), cancellationToken));

    [HttpGet("tasks")]
    public async Task<IActionResult> Tasks(
        [FromQuery] Guid? leadId,
        [FromQuery] string? status,
        [FromQuery] Guid? responsibleUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetCrmTasksQuery(leadId, status, responsibleUserId, page, pageSize),
            cancellationToken));

    [HttpPost("leads/{leadId:guid}/tasks")]
    [Authorize(Policy = "CrmManage")]
    public async Task<IActionResult> CreateTask(Guid leadId, [FromBody] CreateCrmTaskRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new CreateCrmTaskCommand(leadId, request.Title, request.Description, request.DueDate,
                request.ResponsibleUserId), cancellationToken));

    [HttpPost("tasks/{taskId:guid}/complete")]
    [Authorize(Policy = "CrmManage")]
    public async Task<IActionResult> CompleteTask(Guid taskId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CompleteCrmTaskCommand(taskId), cancellationToken));

    [HttpPost("leads/{leadId:guid}/activities")]
    [Authorize(Policy = "CrmManage")]
    public async Task<IActionResult> AddActivity(Guid leadId, [FromBody] AddCrmActivityRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new AddCrmActivityCommand(leadId, request.Type, request.Description, request.RelatedEntityId,
                request.RelatedEntityType), cancellationToken));

    [HttpGet("leads/{leadId:guid}/timeline")]
    public async Task<IActionResult> Timeline(Guid leadId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCrmTimelineQuery(leadId), cancellationToken));
}