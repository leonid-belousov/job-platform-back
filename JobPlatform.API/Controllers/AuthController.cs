using JobPlatform.API.Extensions;
using JobPlatform.BLL.CQRS.Auth.Commands.ConfirmEmail;
using JobPlatform.BLL.CQRS.Auth.Commands.ForgotPassword;
using JobPlatform.BLL.CQRS.Auth.Commands.RefreshToken;
using JobPlatform.BLL.CQRS.Auth.Commands.ResendEmailConfirmation;
using JobPlatform.BLL.CQRS.Auth.Commands.ResetPassword;
using JobPlatform.BLL.CQRS.Auth.Commands.RevokeAllSessions;
using JobPlatform.BLL.CQRS.Auth.Commands.RevokeSession;
using JobPlatform.BLL.CQRS.Auth.Commands.SignIn;
using JobPlatform.BLL.CQRS.Auth.Commands.SignOut;
using JobPlatform.BLL.CQRS.Auth.Commands.SignUp;
using JobPlatform.BLL.CQRS.Auth.Queries.GetMySessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { IpAddress = GetClientIp() }, cancellationToken);
        return Ok(result);
    }

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { IpAddress = GetClientIp() }, cancellationToken);
        return NoContent();
    }

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { IpAddress = GetClientIp() }, cancellationToken);
        return NoContent();
    }

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
    
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody]RefreshTokenRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, GetClientIp()), cancellationToken));

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
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