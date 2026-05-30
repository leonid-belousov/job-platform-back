using JobPlatform.BLL.CQRS.Dictionaries.Commands.CreateDictionaryItem;
using JobPlatform.BLL.CQRS.Dictionaries.Commands.SetDictionaryItemActive;
using JobPlatform.BLL.CQRS.Dictionaries.Commands.UpdateDictionaryItem;
using JobPlatform.BLL.CQRS.Dictionaries.Queries.GetDictionaryItems;
using JobPlatform.BLL.CQRS.Dictionaries.Queries.GetDictionaryTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DictionariesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DictionariesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetDictionaryTypesQuery(), cancellationToken));

    [HttpGet("{type}")]
    public async Task<IActionResult> GetItems(
        string type,
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetDictionaryItemsQuery(type, activeOnly, search), cancellationToken));

    [Authorize(Policy = "DictionariesManage")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody]CreateDictionaryItemCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [Authorize(Policy = "DictionariesManage")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDictionaryItemRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new UpdateDictionaryItemCommand(id, request.Code, request.Name, request.Description, request.SortOrder,
                request.IsActive), cancellationToken));

    [Authorize(Policy = "DictionariesManage")]
    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetDictionaryItemActiveRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new SetDictionaryItemActiveCommand(id, request.IsActive), cancellationToken));
}

public sealed record UpdateDictionaryItemRequest(
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive);

public sealed record SetDictionaryItemActiveRequest(bool IsActive);