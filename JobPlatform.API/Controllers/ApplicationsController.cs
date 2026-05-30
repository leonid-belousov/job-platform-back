using JobPlatform.BLL.CQRS.Applications.Commands.CancelInterviewInvitation;
using JobPlatform.BLL.CQRS.Applications.Commands.ChangeApplicationStatus;
using JobPlatform.BLL.CQRS.Applications.Commands.CreateApplication;
using JobPlatform.BLL.CQRS.Applications.Commands.CreateInterviewInvitation;
using JobPlatform.BLL.CQRS.Applications.Commands.RespondInterviewInvitation;
using JobPlatform.BLL.CQRS.Applications.Queries.GetApplicationInterviewInvitations;
using JobPlatform.BLL.CQRS.Applications.Queries.GetCandidateApplications;
using JobPlatform.BLL.CQRS.Applications.Queries.GetVacancyApplications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Apply(CreateApplicationCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPatch("{applicationId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid applicationId, ChangeApplicationStatusCommand command,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command with { ApplicationId = applicationId }, cancellationToken));

    [HttpGet("my")]
    public async Task<IActionResult> My(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCandidateApplicationsQuery(), cancellationToken));

    [HttpGet("by-vacancy/{vacancyId:guid}")]
    public async Task<IActionResult> ByVacancy(Guid vacancyId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetVacancyApplicationsQuery(vacancyId), cancellationToken));

    [HttpPost("{applicationId:guid}/interview-invitations")]
    public async Task<IActionResult> CreateInterviewInvitation(Guid applicationId,
        [FromBody] CreateInterviewInvitationRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CreateInterviewInvitationCommand(applicationId, request.ScheduledAt,
            request.Format, request.Location, request.MeetingUrl, request.Message), cancellationToken));

    [HttpGet("{applicationId:guid}/interview-invitations")]
    public async Task<IActionResult> GetInterviewInvitations(Guid applicationId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetApplicationInterviewInvitationsQuery(applicationId), cancellationToken));

    [HttpPost("interview-invitations/{invitationId:guid}/accept")]
    public async Task<IActionResult> AcceptInterviewInvitation(Guid invitationId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RespondInterviewInvitationCommand(invitationId, "accepted"), cancellationToken));

    [HttpPost("interview-invitations/{invitationId:guid}/decline")]
    public async Task<IActionResult> DeclineInterviewInvitation(Guid invitationId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RespondInterviewInvitationCommand(invitationId, "declined"), cancellationToken));

    [HttpPost("interview-invitations/{invitationId:guid}/cancel")]
    public async Task<IActionResult> CancelInterviewInvitation(Guid invitationId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CancelInterviewInvitationCommand(invitationId), cancellationToken));
}

public sealed record CreateInterviewInvitationRequest(
    DateTimeOffset ScheduledAt,
    string Format,
    string? Location,
    string? MeetingUrl,
    string? Message);