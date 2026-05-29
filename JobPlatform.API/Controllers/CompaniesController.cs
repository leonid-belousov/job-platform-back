using JobPlatform.BLL.CQRS.Companies.Commands.CreateCompany;
using JobPlatform.BLL.CQRS.Companies.Commands.UpdateCompany;
using JobPlatform.BLL.CQRS.Companies.Queries.GetMyCompanies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CompaniesManage")]
public sealed class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCompanyCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPut("{companyId:guid}")]
    public async Task<IActionResult> Update(Guid companyId, UpdateCompanyCommand command,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command with { CompanyId = companyId }, cancellationToken));

    [HttpGet("my")]
    public async Task<IActionResult> My(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyCompaniesQuery(), cancellationToken));
}