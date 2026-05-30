using JobPlatform.BLL.CQRS.Vacancies.Commands.ArchiveVacancy;
using JobPlatform.BLL.CQRS.Vacancies.Commands.AssignRecruiter;
using JobPlatform.BLL.CQRS.Vacancies.Commands.CreateVacancy;
using JobPlatform.BLL.CQRS.Vacancies.Commands.PublishVacancy;
using JobPlatform.BLL.CQRS.Vacancies.Commands.UnassignRecruiter;
using JobPlatform.BLL.CQRS.Vacancies.Queries.GetMyCompanyVacancies;
using JobPlatform.BLL.CQRS.Vacancies.Queries.GetVacancyRecruiters;
using JobPlatform.BLL.CQRS.Vacancies.Queries.SearchVacancies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VacanciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VacanciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = "VacanciesManage")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody]CreateVacancyCommand command, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(command, cancellationToken));

    [Authorize(Policy = "VacanciesManage")]
    [HttpPost("{vacancyId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid vacancyId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new PublishVacancyCommand(vacancyId), cancellationToken));

    [Authorize(Policy = "VacanciesManage")]
    [HttpPost("{vacancyId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid vacancyId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ArchiveVacancyCommand(vacancyId), cancellationToken));

    [Authorize(Policy = "VacanciesManage")]
    [HttpGet("by-company/{companyId:guid}")]
    public async Task<IActionResult> ByCompany(Guid companyId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyCompanyVacanciesQuery(companyId), cancellationToken));

    [Authorize(Policy = "VacanciesManage")]
    [HttpGet("{vacancyId:guid}/recruiters")]
    public async Task<IActionResult> Recruiters(Guid vacancyId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetVacancyRecruitersQuery(vacancyId), cancellationToken));

    [Authorize(Policy = "VacanciesManage")]
    [HttpPost("{vacancyId:guid}/recruiters")]
    public async Task<IActionResult> AssignRecruiter(Guid vacancyId, [FromBody] AssignRecruiterRequest request,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AssignRecruiterToVacancyCommand(vacancyId, request.RecruiterUserId),
            cancellationToken));

    [Authorize(Policy = "VacanciesManage")]
    [HttpDelete("{vacancyId:guid}/recruiters/{recruiterUserId:guid}")]
    public async Task<IActionResult> UnassignRecruiter(Guid vacancyId, Guid recruiterUserId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnassignRecruiterFromVacancyCommand(vacancyId, recruiterUserId), cancellationToken);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? text,
        [FromQuery] string? country,
        [FromQuery] string? city,
        [FromQuery] decimal? salaryFrom,
        [FromQuery] decimal? salaryTo,
        [FromQuery] string? employmentType,
        [FromQuery] string? workFormat,
        [FromQuery] string? experienceLevel,
        [FromQuery] string? currency,
        [FromQuery] string? sortBy = "date",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(
            new SearchVacanciesQuery(text, country, city, salaryFrom, salaryTo, employmentType, workFormat,
                experienceLevel, currency, sortBy, page, pageSize),
            cancellationToken));
}

public sealed record AssignRecruiterRequest(Guid RecruiterUserId);