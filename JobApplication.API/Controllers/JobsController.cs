namespace JobApplication.API.Controllers;

using JobApplication.Application.DTOs.Jobs;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Features.Jobs.Commands.CreateJob;
using JobApplication.Application.Features.Jobs.Commands.DeleteJob;
using JobApplication.Application.Features.Jobs.Commands.UpdateJob;
using JobApplication.Application.Features.Jobs.Queries.GetAllJobs;
using JobApplication.Application.Features.Jobs.Queries.GetJobById;
using JobApplication.Application.Features.Jobs.Queries.GetMyJobs;
using JobApplication.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class JobsController(IMediator mediator) : ControllerBase
{

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<JobResponse>> Create(
         CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var command = new CreateJobCommand(
            request.Title,
            request.Description,
            request.EmploymentType,
            userId);

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { jobId = result.Value.Id }, result.Value);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<JobResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var jobs = await mediator.Send(new GetAllJobsQuery(), cancellationToken);
        return Ok(jobs.Value);
    }

    [HttpGet("{jobId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobResponse>> GetById(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var job = await mediator.Send(new GetJobByIdQuery(jobId), cancellationToken);
        return Ok(job.Value);
    }

    [HttpGet("my-jobs")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<IReadOnlyCollection<JobResponse>>> GetMyJobs(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var jobs = await mediator.Send(new GetMyJobsQuery(userId), cancellationToken);
        return Ok(jobs.Value);
    }

    [HttpPut("{jobId:int}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<JobResponse>> Update(
        [FromRoute] int jobId,
        [FromBody] UpdateJobRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new UpdateJobCommand(
            request.Title,
            request.Description,
            request.EmploymentType,
            jobId,
            userId);

        var updatedJob = await mediator.Send(command, cancellationToken);
        return Ok(updatedJob.Value);
    }

    [HttpDelete("{jobId:int}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Delete(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new DeleteJobCommand(jobId, userId), cancellationToken);
        return Ok(result.IsSuccess);
    }

    [HttpPatch("{jobId:int}/close")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Close(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new CloseJobCommand(jobId, userId), cancellationToken);
        return Ok(result.IsSuccess);
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
