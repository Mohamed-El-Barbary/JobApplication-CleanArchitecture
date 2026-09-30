namespace JobApplication.Application.Features.Auth.Commands.ResendVerificationEmail;

using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

internal class ResendVerificationEmailCommandHandler(
    IIdentityService identityService,
    IBackgroundJobClient backgroundJobClient,
    IOtpService otpService) : IRequestHandler<ResendVerificationEmailCommand, Result>
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

        // Generate and store email verification OTP
        var otp = await otpService.GenerateAsync(
            OtpPurpose.EmailVerification,
            user.Email!,
            ck: cancellationToken);

        // Send OTP asynchronously using Hangfire
        backgroundJobClient.Enqueue<IEmailService>(
            x => x.SendOtpEmailAsync(
                user.Email!,
                otp,
                OtpPurpose.EmailVerification,
                CancellationToken.None));

        return Result.Success();
    }
}
