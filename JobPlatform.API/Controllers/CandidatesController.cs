using JobPlatform.BLL.CQRS.Candidates.Commands;
using JobPlatform.BLL.CQRS.Candidates.Commands.CreateOrUpdateProfile;
using JobPlatform.BLL.CQRS.Candidates.Commands.CreateResume;
using JobPlatform.BLL.CQRS.Candidates.Queries.GetMyCandidateProfile;
using JobPlatform.BLL.CQRS.Candidates.Queries.GetMyResumes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CandidatesManageOwn")]
public sealed class CandidatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("profile")]
    public async Task<IActionResult> CreateOrUpdateProfile([FromBody] CreateOrUpdateCandidateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("me/profile")]
    public async Task<IActionResult> MyProfile(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyCandidateProfileQuery(), cancellationToken));

    [HttpPost("me/resumes")]
    public async Task<IActionResult> CreateResume(CreateResumeCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet("me/resumes")]
    public async Task<IActionResult> MyResumes(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyResumesQuery(), cancellationToken));
}