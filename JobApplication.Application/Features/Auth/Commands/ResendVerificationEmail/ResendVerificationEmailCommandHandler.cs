namespace JobApplication.Application.Features.Auth.Commands.ResendVerificationEmail;

using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

internal class ResendVerificationEmailCommandHandler(
    IIdentityService identityService,
    IBackgroundJobClient backgroundJobClient,
    IConfiguration configuration) : IRequestHandler<ResendVerificationEmailCommand, Result>
{
    public async Task<Result> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure(Error.Validation("Auth.ResendVerification.EmailRequired", "Email is required."));

        var userResult = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userResult.IsFailure)
            return Result.Success(); // Do not reveal if email exists

        var user = userResult.Value;

        if (await identityService.IsEmailConfirmedAsync(user, cancellationToken))
            return Result.Success(); // Do not reveal if already verified

        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(user, cancellationToken);
        if (tokenResult.IsFailure)
            return Result.Success();

        var encodedToken = HttpUtility.UrlEncode(tokenResult.Value);
        var baseUrl = configuration["Frontend:BaseUrl"];
        var verificationLink = $"{baseUrl}/verify-email?userId={user.Id}&token={encodedToken}";

        backgroundJobClient.Enqueue<IEmailService>(x => x.SendVerificationEmailAsync(
            user.Email!,
            verificationLink,
            CancellationToken.None));

        return Result.Success();
    }
}
