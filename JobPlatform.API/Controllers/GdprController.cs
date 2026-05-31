using JobPlatform.BLL.CQRS.Gdpr.Commands.AnonymizeMyPersonalData;
using JobPlatform.BLL.CQRS.Gdpr.Queries.ExportMyPersonalData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class GdprController : ControllerBase
{
    private readonly IMediator _mediator;

    public GdprController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me/export")]
    public async Task<IActionResult> ExportMyPersonalData(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ExportMyPersonalDataQuery(), cancellationToken));

    [HttpDelete("me/anonymize")]
    public async Task<IActionResult> AnonymizeMyPersonalData([FromBody] AnonymizeMyPersonalDataRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AnonymizeMyPersonalDataCommand(request.Confirm), cancellationToken);
        return NoContent();
    }
}

public sealed record AnonymizeMyPersonalDataRequest(bool Confirm);
