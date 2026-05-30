using JobPlatform.BLL.CQRS.Notifications.Commands.MarkAllNotificationsAsRead;
using JobPlatform.BLL.CQRS.Notifications.Commands.MarkNotificationAsRead;
using JobPlatform.BLL.CQRS.Notifications.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool? isRead, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetMyNotificationsQuery(isRead, page, pageSize), cancellationToken));

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationAsReadCommand(notificationId), cancellationToken);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        => Ok(new { updated = await _mediator.Send(new MarkAllNotificationsAsReadCommand(), cancellationToken) });
}