namespace JobApplication.Application.Features.Auth.Commands.RefreshToken;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Identity;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class RefreshTokenCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponse>>
{
    public async Task<Result<AuthenticationResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return Error.Validation("Auth.RefreshToken.Required", "RefreshToken is required.");

        var userResult = await identityService.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (userResult.IsFailure)
            return userResult.Error!;

        var user = userResult.Value;

        var revokeResult = await identityService.RevokeRefreshTokenAsync(user, request.RefreshToken, cancellationToken);
        if (revokeResult.IsFailure)
            return revokeResult.Error!;

        var rolesResult = await identityService.GetRolesAsync(user, cancellationToken);
        var roles = rolesResult.IsSuccess ? rolesResult.Value : Array.Empty<string>();

        var (accessToken, accessTokenExpiresAt) = jwtTokenGenerator.GenerateAccessToken(user, roles);
        var (refreshTokenString, refreshTokenExpiresAt) = jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenString,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = refreshTokenExpiresAt
        };

        var addRefreshTokenResult = await identityService.AddRefreshTokenAsync(user, refreshTokenEntity, cancellationToken);
        if (addRefreshTokenResult.IsFailure)
            return addRefreshTokenResult.Error!;

        var authUserDto = new AuthUserDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Roles = roles,
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshTokenString,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };

        return authUserDto.Adapt<AuthenticationResponse>();
    }
}
