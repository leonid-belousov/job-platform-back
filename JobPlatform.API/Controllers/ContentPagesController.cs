using JobPlatform.BLL.CQRS.ContentPages.Queries.GetContentPage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/content-pages")]
public sealed class ContentPagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContentPagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetContentPageQuery(slug), cancellationToken);
        return page is null ? NotFound() : Ok(page);
    }
}
