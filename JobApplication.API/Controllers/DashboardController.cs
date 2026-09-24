using JobApplication.Application.DTOs.Dashboard;
using JobApplication.Application.Features.Dashboard.Queries.GetCandidateDashboard;
using JobApplication.Application.Features.Dashboard.Queries.GetRecruiterDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateDashboardResponse>> GetCandidateDashboard(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetCandidateDashboardQuery(userId), cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("recruiter")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<RecruiterDashboardResponse>> GetRecruiterDashboard(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetRecruiterDashboardQuery(userId), cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    private Guid GetUserId()
    {
        return Guid.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
