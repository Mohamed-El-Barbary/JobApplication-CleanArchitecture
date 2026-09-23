using JobApplication.Application.DTOs.Interviews;
using JobApplication.Application.Features.Interviews.Commands.CancelInterview;
using JobApplication.Application.Features.Interviews.Commands.RescheduleInterview;
using JobApplication.Application.Features.Interviews.Commands.ScheduleInterview;
using JobApplication.Application.Features.Interviews.Commands.UpdateInterview;
using JobApplication.Application.Features.Interviews.Queries.GetApplicationInterviews;
using JobApplication.Application.Features.Interviews.Queries.GetInterviewById;
using JobApplication.Application.Features.Interviews.Queries.GetMyInterviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers;

[ApiController]
public class InterviewsController(IMediator mediator) : ControllerBase
{
    [HttpPost("api/job-applications/{applicationId:int}/interviews")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<InterviewResponse>> Schedule(
        [FromRoute] int applicationId,
        [FromBody] CreateInterviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new ScheduleInterviewCommand(
            request.ScheduledAt,
            request.Duration,
            request.Type,
            request.MeetingUrl,
            request.Notes,
            applicationId,
            userId);

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { interviewId = result.Value.Id }, result.Value);   
    }

    [HttpGet("api/job-applications/{applicationId:int}/interviews")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InterviewResponse>>> GetApplicationInterviews(
        [FromRoute] int applicationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var query = new GetApplicationInterviewsQuery(applicationId, userId);
        var result = await mediator.Send(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("api/interviews/{interviewId:int}")]
    [Authorize]
    public async Task<ActionResult<InterviewResponse>> GetById(
        [FromRoute] int interviewId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var query = new GetInterviewByIdQuery(interviewId, userId);
        var result = await mediator.Send(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPut("api/interviews/{interviewId:int}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<InterviewResponse>> Update(
        [FromRoute] int interviewId,
        [FromBody] UpdateInterviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new UpdateInterviewCommand(
            request.ScheduledAt,
            request.Duration,
            request.Type,
            request.MeetingUrl,
            request.Notes,
            interviewId,
            userId);

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPatch("api/interviews/{interviewId:int}/reschedule")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Reschedule(
        [FromRoute] int interviewId,
        [FromBody] RescheduleInterviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new RescheduleInterviewCommand(
            request.ScheduledAt,
            interviewId,
            userId);

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.IsSuccess);
    }

    [HttpPatch("api/interviews/{interviewId:int}/cancel")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Cancel(
        [FromRoute] int interviewId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new CancelInterviewCommand(interviewId, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.IsSuccess);
    }

    [HttpGet("api/interviews/my-interviews")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InterviewResponse>>> GetMyInterviews(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var query = new GetMyInterviewsQuery(userId);
        var result = await mediator.Send(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

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
