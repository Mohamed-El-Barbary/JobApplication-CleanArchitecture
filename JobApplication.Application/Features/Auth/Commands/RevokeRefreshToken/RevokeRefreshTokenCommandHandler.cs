namespace JobApplication.Application.Features.Auth.Commands.RevokeRefreshToken;

using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal class RevokeRefreshTokenCommandHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService) : IRequestHandler<RevokeRefreshTokenCommand, Result>
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Failure(Error.Validation("Auth.Revoke.RefreshTokenRequired", "Refresh token is required."));

        if (!currentUserService.IsAuthenticated || currentUserService.UserId == null)
            return Result.Failure(Error.Unauthorized("Auth.Revoke.Unauthorized", "User is not authenticated."));

        var userResult = await identityService.GetUserByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (userResult.IsFailure)
            return Result.Failure(userResult.Error!);

        return await identityService.RevokeRefreshTokenAsync(userResult.Value, request.RefreshToken, cancellationToken);
    }
}
