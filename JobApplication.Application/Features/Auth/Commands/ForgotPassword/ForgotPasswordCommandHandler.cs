namespace JobApplication.Application.Features.Auth.Commands.ForgotPassword;

using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

internal class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IBackgroundJobClient backgroundJobClient,
    IOtpService otpService) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure(Error.Validation("Auth.ForgotPassword.EmailRequired", "Email is required."));

        var userResult = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);

        // Do not reveal whether the email exists. Always return success.
        if (userResult.IsFailure)
            return Result.Success();

        var user = userResult.Value;

        var otp = await otpService.GenerateAsync(
           OtpPurpose.PasswordReset,
           user.Email!,
           ck: cancellationToken);

        backgroundJobClient.Enqueue<IEmailService>(
            x => x.SendOtpEmailAsync(
                user.Email!,
                otp,
                OtpPurpose.PasswordReset,
                CancellationToken.None));

        return Result.Success();
    }
}
