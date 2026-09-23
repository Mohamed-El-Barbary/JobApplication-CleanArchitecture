using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Application.Features.JobApplications.Commands.CancelJobApplication;
using JobApplication.Application.Features.JobApplications.Commands.CreateJobApplication;
using JobApplication.Application.Features.JobApplications.Commands.UpdateJobApplication;
using JobApplication.Application.Features.JobApplications.Commands.UpdateJobApplicationStatus;
using JobApplication.Application.Features.JobApplications.Queries.GetCandidateJobApplications;
using JobApplication.Application.Features.JobApplications.Queries.GetJobApplicationById;
using JobApplication.Application.Features.JobApplications.Queries.GetRecruiterJobApplications;
using JobApplication.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers;

[ApiController]
public class JobApplicationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("api/jobs/{jobId:int}/applications")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<JobApplicationResponse>> Apply(
        [FromRoute] int jobId,
        [FromBody] CreateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new CreateJobApplicationCommand(
            request.CoverLetter,
            request.ResumeUrl,
            request.YearsOfExperience,
            jobId,
            userId
            );
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { applicationId = result.Value.Id }, result.Value);
    }

    [HttpGet("api/job-applications/my-applications")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<IReadOnlyCollection<JobApplicationResponse>>> GetMyApplications(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var applications = await mediator.Send(new GetMyJobApplicationsQuery(userId), cancellationToken);
        return Ok(applications.Value);
    }

    [HttpGet("api/job-applications/{applicationId:int}")]
    [Authorize]
    public async Task<ActionResult<JobApplicationResponse>> GetById(
        [FromRoute] int applicationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var application = await mediator.Send(new GetJobApplicationByIdQuery(applicationId, userId), cancellationToken);
        return Ok(application.Value);
    }

    [HttpPut("api/job-applications/{applicationId:int}")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<JobApplicationResponse>> Update(
        [FromRoute] int applicationId,
        [FromBody] UpdateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new UpdateJobApplicationCommand(
            request.CoverLetter,
            request.ResumeUrl,
            request.YearsOfExperience,
            applicationId,
            userId
            );

        var updatedApplication = await mediator.Send(command, cancellationToken);
        return Ok(updatedApplication);
    }

    [HttpPatch("api/job-applications/{applicationId:int}/cancel")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Cancel(
        [FromRoute] int applicationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new CancelJobApplicationCommand(applicationId, userId), cancellationToken);
        return Ok(result.IsSuccess);
    }

    [HttpGet("api/jobs/{jobId:int}/applications")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<IReadOnlyCollection<JobApplicationResponse>>> GetJobApplications(
        [FromRoute] int jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var applications = await mediator.Send(new GetRecruiterJobApplicationsQuery(jobId, userId), cancellationToken);
        return Ok(applications.Value);
    }

    [HttpPatch("api/job-applications/{applicationId:int}/status")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<JobApplicationResponse>> UpdateStatus(
        [FromRoute] int applicationId,
        [FromBody] UpdateApplicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new UpdateJobApplicationStatusCommand(
            request.Status,
            applicationId,
            userId);

        var updatedApplication = await mediator.Send(command, cancellationToken);
        return Ok(updatedApplication.Value);
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
