namespace JobApplication.Application.Features.Auth.Commands.Register;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Entities.Identity;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using System.Web;
using Hangfire;
using Microsoft.Extensions.Configuration;

internal class RegisterCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork,
    IBackgroundJobClient backgroundJobClient,
    IConfiguration configuration) : IRequestHandler<RegisterCommand, Result<AuthenticationResponse>>
{
    public async Task<Result<AuthenticationResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
            return Error.Validation("Auth.Register.Empty", "Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return Error.Validation("Auth.Register.FullNameRequired", "FullName is required.");

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            return Error.Validation("Auth.Register.EmailInvalid", "A valid Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Error.Validation("Auth.Register.PasswordRequired", "Password is required.");

        if (request.Password != request.ConfirmPassword)
            return Error.Validation("Auth.Register.PasswordMismatch", "Password and ConfirmPassword do not match.");

        if (string.IsNullOrWhiteSpace(request.Role))
            return Error.Validation("Auth.Register.RoleRequired", "Role is required.");

        var registerResult = await identityService.RegisterUserAsync(request.Email, request.Password, request.FullName, request.Role, cancellationToken);

        if (registerResult.IsFailure)
            return registerResult.Error!;

        var user = registerResult.Value;
        string assignedRole = request.Role.Equals("Candidate", StringComparison.OrdinalIgnoreCase) ? "Candidate" : "Recruiter";

        try
        {
            if (assignedRole == "Candidate")
            {
                var candidate = new Candidate
                {
                    FullName = user.FullName,
                    Email = user.Email!,
                    CVUrl = string.Empty,
                    UserId = user.Id
                };
                await unitOfWork.Repository<Candidate>().AddAsync(candidate);
            }
            else
            {
                var recruiter = new Recruiter
                {
                    FullName = user.FullName,
                    Email = user.Email!,
                    UserId = user.Id
                };
                await unitOfWork.Repository<Recruiter>().AddAsync(recruiter);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await identityService.DeleteUserAsync(user, cancellationToken);
            throw;
        }

        var rolesResult = await identityService.GetRolesAsync(user, cancellationToken);
        var roles = rolesResult.IsSuccess ? rolesResult.Value : new List<string> { assignedRole };

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
        {
            return addRefreshTokenResult.Error!;
        }

        // Generate Email Verification Token
        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(user, cancellationToken);
        if (tokenResult.IsSuccess)
        {
            var encodedToken = HttpUtility.UrlEncode(tokenResult.Value);
            var baseUrl = configuration["Frontend:BaseUrl"];
            var verificationLink = $"{baseUrl}/verify-email?userId={user.Id}&token={encodedToken}";

            backgroundJobClient.Enqueue<IEmailService>(x => x.SendVerificationEmailAsync(
                user.Email!,
                verificationLink,
                CancellationToken.None));
        }

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
