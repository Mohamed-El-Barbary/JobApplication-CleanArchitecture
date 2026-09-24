namespace JobApplication.Application.Features.Auth.Commands.ChangePassword;

using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal class ChangePasswordCommandHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return Result.Failure(Error.Validation("Auth.ChangePassword.CurrentPasswordRequired", "Current password is required."));

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return Result.Failure(Error.Validation("Auth.ChangePassword.NewPasswordRequired", "New password is required."));

        if (request.NewPassword != request.ConfirmNewPassword)
            return Result.Failure(Error.Validation("Auth.ChangePassword.PasswordMismatch", "New passwords do not match."));

        if (!currentUserService.IsAuthenticated || currentUserService.UserId == null)
            return Result.Failure(Error.Unauthorized("Auth.ChangePassword.Unauthorized", "User is not authenticated."));

        var userResult = await identityService.GetUserByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (userResult.IsFailure)
            return Result.Failure(userResult.Error!);

        var changePasswordResult = await identityService.ChangePasswordAsync(userResult.Value, request.CurrentPassword, request.NewPassword, cancellationToken);
        if (changePasswordResult.IsFailure)
            return Result.Failure(changePasswordResult.Error!);

        // Optionally revoke existing refresh tokens upon password change
        await identityService.RevokeAllRefreshTokensAsync(userResult.Value, cancellationToken);

        return Result.Success();
    }
}
