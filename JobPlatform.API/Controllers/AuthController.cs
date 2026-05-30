using JobPlatform.BLL.CQRS.Auth.Commands.RefreshToken;
using JobPlatform.BLL.CQRS.Auth.Commands.RevokeAllSessions;
using JobPlatform.BLL.CQRS.Auth.Commands.RevokeSession;
using JobPlatform.BLL.CQRS.Auth.Commands.SignIn;
using JobPlatform.BLL.CQRS.Auth.Commands.SignOut;
using JobPlatform.BLL.CQRS.Auth.Commands.SignUp;
using JobPlatform.BLL.CQRS.Auth.Queries.GetMySessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody]RefreshTokenRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, GetClientIp()), cancellationToken));

    [HttpPost("sign-out")]
    public async Task<IActionResult> Logout([FromBody]SignOutCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetMySessions(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMySessionsQuery(), cancellationToken));

    [Authorize]
    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeSessionCommand(sessionId), cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeAllSessionsCommand(), cancellationToken);
        return NoContent();
    }

    private string? GetClientIp()
        => HttpContext.Connection.RemoteIpAddress?.ToString();
}

public sealed record RefreshTokenRequest(string RefreshToken);