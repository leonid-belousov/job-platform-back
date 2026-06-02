using JobPlatform.BLL.CQRS.Feedback.Commands.SendFeedback;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FeedbackController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeedbackController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendFeedbackCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Accepted(new { message = "Feedback request accepted." });
    }
}
