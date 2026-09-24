namespace JobApplication.Application.Features.Auth.Commands.Logout;

using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal class LogoutCommandHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Failure(Error.Validation("Auth.Logout.RefreshTokenRequired", "Refresh token is required."));

        if (!currentUserService.IsAuthenticated || currentUserService.UserId == null)
            return Result.Failure(Error.Unauthorized("Auth.Logout.Unauthorized", "User is not authenticated."));

        var userResult = await identityService.GetUserByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (userResult.IsFailure)
            return Result.Failure(userResult.Error!);

        return await identityService.RevokeRefreshTokenAsync(userResult.Value, request.RefreshToken, cancellationToken);
    }
}
