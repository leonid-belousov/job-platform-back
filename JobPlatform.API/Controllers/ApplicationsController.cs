using JobPlatform.BLL.CQRS.Applications.Commands.ChangeApplicationStatus;
using JobPlatform.BLL.CQRS.Applications.Commands.CreateApplication;
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
}