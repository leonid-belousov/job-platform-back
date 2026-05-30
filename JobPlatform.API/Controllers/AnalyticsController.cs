using JobPlatform.BLL.CQRS.Analytics.Queries.GetEmployerDashboard;
using JobPlatform.BLL.CQRS.Analytics.Queries.GetRecruiterDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AnalyticsRead")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("employer/companies/{companyId:guid}/dashboard")]
    public async Task<IActionResult> EmployerDashboard(Guid companyId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetEmployerDashboardQuery(companyId), cancellationToken));

    [HttpGet("recruiter/dashboard")]
    public async Task<IActionResult> RecruiterDashboard(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetRecruiterDashboardQuery(), cancellationToken));
}