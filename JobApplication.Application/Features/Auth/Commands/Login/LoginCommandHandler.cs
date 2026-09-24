namespace JobApplication.Application.Features.Auth.Commands.Login;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Identity;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    public async Task<Result<AuthenticationResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
            return Error.Validation("Auth.Login.Empty", "Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Error.Validation("Auth.Login.EmailRequired", "Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Error.Validation("Auth.Login.PasswordRequired", "Password is required.");

        var validateResult = await identityService.ValidateUserAsync(request.Email, request.Password, cancellationToken);
        if (validateResult.IsFailure)
            return validateResult.Error!;

        var user = validateResult.Value;

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
