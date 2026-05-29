using JobPlatform.BLL.CQRS.Files.Commands.UploadFile;
using JobPlatform.BLL.CQRS.Files.Queries.GetFileDownload;
using JobPlatform.BLL.CQRS.Files.Queries.GetFileMetadata;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest("Файл пустой.");

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new UploadFileCommand(
            stream,
            file.FileName,
            file.ContentType,
            file.Length), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> Metadata(Guid fileId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetFileMetadataQuery(fileId), cancellationToken));

    [HttpGet("{fileId:guid}/download")]
    public async Task<IActionResult> Download(Guid fileId, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(new GetFileDownloadQuery(fileId), cancellationToken);
        return File(file.Content, file.ContentType, file.OriginalName);
    }
}
