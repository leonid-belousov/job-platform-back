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
}