using JobApplication.Application.Features.Auth.Commands.ChangePassword;
using JobApplication.Application.Features.Auth.Commands.ForgotPassword;
using JobApplication.Application.Features.Auth.Commands.Login;
using JobApplication.Application.Features.Auth.Commands.Logout;
using JobApplication.Application.Features.Auth.Commands.RefreshToken;
using JobApplication.Application.Features.Auth.Commands.Register;
using JobApplication.Application.Features.Auth.Commands.ResendVerificationEmail;
using JobApplication.Application.Features.Auth.Commands.ResetPassword;
using JobApplication.Application.Features.Auth.Commands.RevokeRefreshToken;
using JobApplication.Application.Features.Auth.Commands.VerifyEmail;
using JobApplication.Application.Features.Auth.Queries.GetCurrentUser;
using JobApplication.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok(result.Value);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok(result.Value);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok();
    }

    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmail(
        [FromBody] ResendVerificationEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok();
    }


    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok(result.Value);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok();
    }

    [HttpPost("revoke-refresh-token")]
    [Authorize]
    public async Task<IActionResult> RevokeRefreshToken(
        [FromBody] RevokeRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error!) : Ok();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult ToErrorResponse(Error error) => error.Type switch
    {
        ErrorType.Validation  => BadRequest(error),
        ErrorType.NotFound    => NotFound(error),
        ErrorType.Conflict    => Conflict(error),
        ErrorType.Unauthorized => Unauthorized(error),
        ErrorType.Forbidden   => StatusCode(403, error),
        _                     => StatusCode(500, error)
    };
}
