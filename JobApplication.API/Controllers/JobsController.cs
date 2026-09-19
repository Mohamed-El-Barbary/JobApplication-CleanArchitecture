namespace JobApplication.API.Controllers;

using JobApplication.Application.DTOs.Jobs;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class JobsController(IJobService jobService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<JobResponse>> Create(
         CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await jobService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { jobId = result.Id }, result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<JobResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var jobs = await jobService.GetAllAsync(cancellationToken);
        return Ok(jobs);
    }

    [HttpGet("{jobId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobResponse>> GetById(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var job = await jobService.GetByIdAsync(jobId, cancellationToken);
        return Ok(job);
    }

    [HttpGet("my-jobs")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<IReadOnlyCollection<JobResponse>>> GetMyJobs(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var jobs = await jobService.GetMyJobsAsync(userId, cancellationToken);
        return Ok(jobs);
    }

    [HttpPut("{jobId:int}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<JobResponse>> Update(
        [FromRoute] int jobId,
        [FromBody] UpdateJobRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var updatedJob = await jobService.UpdateAsync(jobId, request, userId, cancellationToken);
        return Ok(updatedJob);
    }

    [HttpDelete("{jobId:int}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Delete(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await jobService.DeleteAsync(jobId, userId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{jobId:int}/close")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Close(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await jobService.CloseAsync(jobId, userId, cancellationToken);
        return NoContent();
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
