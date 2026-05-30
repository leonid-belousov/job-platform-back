using JobPlatform.BLL.CQRS.Audit.Queries.GetAuditLogById;
using JobPlatform.BLL.CQRS.Audit.Queries.GetAuditLogs;
using JobPlatform.BLL.CQRS.Auth.Commands.RevokeUserSessions;
using JobPlatform.BLL.CQRS.Moderation.Commands.ApproveCompany;
using JobPlatform.BLL.CQRS.Moderation.Commands.ApproveVacancy;
using JobPlatform.BLL.CQRS.Moderation.Commands.RejectCompany;
using JobPlatform.BLL.CQRS.Moderation.Commands.RejectVacancy;
using JobPlatform.BLL.CQRS.Moderation.Queries.GetModerationQueue;
using JobPlatform.BLL.CQRS.Users.Commands.BlockUser;
using JobPlatform.BLL.CQRS.Users.Commands.UnblockUser;
using JobPlatform.BLL.CQRS.Users.Queries.GetUsersList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] string? search, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetUsersListQuery(search, page, pageSize), cancellationToken));

    [HttpPost("users/{userId:guid}/block")]
    public async Task<IActionResult> BlockUser(Guid userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new BlockUserCommand(userId), cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnblockUserCommand(userId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/sessions")]
    public async Task<IActionResult> RevokeUserSessions(Guid userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeUserSessionsCommand(userId), cancellationToken);
        return NoContent();
    }


    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] Guid? userId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(
            new GetAuditLogsQuery(action, entityType, entityId, userId, from, to, page, pageSize), cancellationToken));

    [HttpGet("audit-logs/{auditLogId:guid}")]
    public async Task<IActionResult> AuditLogById(Guid auditLogId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAuditLogByIdQuery(auditLogId), cancellationToken));

    [HttpGet("moderation/queue")]
    public async Task<IActionResult> ModerationQueue(
        [FromQuery] string? entityType,
        [FromQuery] string? moderationStatus,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetModerationQueueQuery(entityType, moderationStatus, search, page, pageSize),
            cancellationToken));

    [HttpPost("companies/{companyId:guid}/approve")]
    public async Task<IActionResult> ApproveCompany(Guid companyId, [FromBody] ModerationDecisionRequest? request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ApproveCompanyCommand(companyId, request?.Comment), cancellationToken));

    [HttpPost("companies/{companyId:guid}/reject")]
    public async Task<IActionResult> RejectCompany(Guid companyId, [FromBody] ModerationDecisionRequest? request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RejectCompanyCommand(companyId, request?.Comment ?? string.Empty),
            cancellationToken));

    [HttpPost("vacancies/{vacancyId:guid}/approve")]
    public async Task<IActionResult> ApproveVacancy(Guid vacancyId, [FromBody] ModerationDecisionRequest? request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ApproveVacancyCommand(vacancyId, request?.Comment), cancellationToken));

    [HttpPost("vacancies/{vacancyId:guid}/reject")]
    public async Task<IActionResult> RejectVacancy(Guid vacancyId, [FromBody] ModerationDecisionRequest? request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RejectVacancyCommand(vacancyId, request?.Comment ?? string.Empty),
            cancellationToken));
}

public sealed record ModerationDecisionRequest(string? Comment);