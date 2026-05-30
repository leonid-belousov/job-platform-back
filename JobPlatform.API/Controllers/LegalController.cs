using JobPlatform.BLL.CQRS.Legal.Commands.AcceptLegalDocument;
using JobPlatform.BLL.CQRS.Legal.Queries.GetActiveLegalDocument;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LegalController : ControllerBase
{
    private readonly IMediator _mediator;

    public LegalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet("documents/{type}/active")]
    public async Task<IActionResult> GetActiveDocument(string type, [FromQuery] string language = "ru",
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetActiveLegalDocumentQuery(type, language), cancellationToken));

    [Authorize]
    [HttpPost("documents/{type}/accept")]
    public async Task<IActionResult> AcceptDocument(string type, [FromBody] AcceptLegalDocumentRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AcceptLegalDocumentCommand(type, request.Version, request.Language, GetClientIp()),
            cancellationToken);
        return NoContent();
    }

    private string? GetClientIp()
        => HttpContext.Connection.RemoteIpAddress?.ToString();
}

public sealed record AcceptLegalDocumentRequest(string Version, string Language);