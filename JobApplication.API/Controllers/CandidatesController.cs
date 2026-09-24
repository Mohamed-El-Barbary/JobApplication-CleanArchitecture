using JobApplication.Application.DTOs.Candidates;
using JobApplication.Application.Features.Candidates.Commands.DeleteCandidateCv;
using JobApplication.Application.Features.Candidates.Commands.UpdateCandidateProfile;
using JobApplication.Application.Features.Candidates.Commands.UploadCandidateCv;
using JobApplication.Application.Features.Candidates.Queries.GetMyCandidateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatesController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateProfileResponse>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetMyCandidateProfileQuery(userId), cancellationToken);
        
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPut("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateProfileResponse>> UpdateMyProfile(
        [FromBody] UpdateCandidateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new UpdateCandidateProfileCommand(request, userId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("me/cv")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateProfileResponse>> UploadCv(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new UploadCandidateCvCommand(file, userId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("me/cv")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateProfileResponse>> DeleteCv(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new DeleteCandidateCvCommand(userId), cancellationToken);

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
