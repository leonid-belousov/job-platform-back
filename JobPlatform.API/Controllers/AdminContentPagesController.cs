using JobPlatform.BLL.CQRS.ContentPages.Commands.UpsertContentPage;
using JobPlatform.BLL.CQRS.ContentPages.DTO;
using JobPlatform.BLL.CQRS.ContentPages.Queries.GetContentPage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/admin/content-pages")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminContentPagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminContentPagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetContentPageQuery(slug, IncludeDraft: true), cancellationToken);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpPut("{slug}")]
    public async Task<IActionResult> Upsert(string slug, [FromBody] UpsertContentPageRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new UpsertContentPageCommand(
            slug,
            request.SchemaVersion,
            request.IsPublished,
            request.Blocks), cancellationToken));
}

public sealed record UpsertContentPageRequest(
    int SchemaVersion,
    bool IsPublished,
    IReadOnlyList<ContentPageBlockDto> Blocks);
