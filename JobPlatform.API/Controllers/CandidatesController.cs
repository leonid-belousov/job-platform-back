using JobPlatform.BLL.CQRS.Candidates.Commands;
using JobPlatform.BLL.CQRS.Candidates.Commands.CreateOrUpdateProfile;
using JobPlatform.BLL.CQRS.Candidates.Commands.CreateResume;
using JobPlatform.BLL.CQRS.Candidates.Queries.GetMyCandidateProfile;
using JobPlatform.BLL.CQRS.Candidates.Queries.GetMyResumes;
using JobPlatform.BLL.CQRS.Candidates.Queries.SearchCandidates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CandidatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = "CandidatesManageOwn")]
    [HttpPost("profile")]
    public async Task<IActionResult> CreateOrUpdateProfile([FromBody] CreateOrUpdateCandidateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "CandidatesManageOwn")]
    [HttpGet("me/profile")]
    public async Task<IActionResult> MyProfile(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyCandidateProfileQuery(), cancellationToken));

    [Authorize(Policy = "CandidatesManageOwn")]
    [HttpPost("me/resumes")]
    public async Task<IActionResult> CreateResume(CreateResumeCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [Authorize(Policy = "CandidatesManageOwn")]
    [HttpGet("me/resumes")]
    public async Task<IActionResult> MyResumes(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyResumesQuery(), cancellationToken));

    [Authorize(Policy = "CandidatesRead")]
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? text,
        [FromQuery] string? skill,
        [FromQuery] string? experienceLevel,
        [FromQuery] string? country,
        [FromQuery] string? language,
        [FromQuery] string? profession,
        [FromQuery] string? jobSearchStatus,
        [FromQuery] string? sortBy = "date",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new SearchCandidatesQuery(text, skill, experienceLevel, country, language,
            profession, jobSearchStatus, sortBy, page, pageSize), cancellationToken));
}