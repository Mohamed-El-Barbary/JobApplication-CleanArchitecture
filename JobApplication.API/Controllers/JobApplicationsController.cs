using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers;

[ApiController]
public class JobApplicationsController(IJobApplicationService jobApplicationService) : ControllerBase
{
    [HttpPost("api/jobs/{jobId:int}/applications")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<JobApplicationResponse>> Apply(
        [FromRoute] int jobId,
        [FromBody] CreateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await jobApplicationService.CreateAsync(jobId, request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { applicationId = result.Id }, result);
    }

    [HttpGet("api/job-applications/my-applications")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<IReadOnlyCollection<JobApplicationResponse>>> GetMyApplications(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var applications = await jobApplicationService.GetMyApplicationsAsync(userId, cancellationToken);
        return Ok(applications);
    }

    [HttpGet("api/job-applications/{applicationId:int}")]
    [Authorize]
    public async Task<ActionResult<JobApplicationResponse>> GetById(
        [FromRoute] int applicationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var application = await jobApplicationService.GetByIdAsync(applicationId, userId, cancellationToken);
        return Ok(application);
    }

    [HttpPut("api/job-applications/{applicationId:int}")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<JobApplicationResponse>> Update(
        [FromRoute] int applicationId,
        [FromBody] UpdateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var updatedApplication = await jobApplicationService.UpdateAsync(applicationId, request, userId, cancellationToken);
        return Ok(updatedApplication);
    }

    [HttpPatch("api/job-applications/{applicationId:int}/cancel")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Cancel(
        [FromRoute] int applicationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await jobApplicationService.CancelAsync(applicationId, userId, cancellationToken);
        return NoContent();
    }

    [HttpGet("api/jobs/{jobId:int}/applications")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<IReadOnlyCollection<JobApplicationResponse>>> GetJobApplications(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var applications = await jobApplicationService.GetJobApplicationsAsync(jobId, userId, cancellationToken);
        return Ok(applications);
    }

    [HttpPatch("api/job-applications/{applicationId:int}/status")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<JobApplicationResponse>> UpdateStatus(
        [FromRoute] int applicationId,
        [FromBody] UpdateApplicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var updatedApplication = await jobApplicationService.UpdateStatusAsync(applicationId, request, userId, cancellationToken);
        return Ok(updatedApplication);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
