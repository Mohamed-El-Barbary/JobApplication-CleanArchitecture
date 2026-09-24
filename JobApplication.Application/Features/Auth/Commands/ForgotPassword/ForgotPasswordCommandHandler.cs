namespace JobApplication.Application.Features.Auth.Commands.ForgotPassword;

using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

internal class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IBackgroundJobClient backgroundJobClient,
    IConfiguration configuration) : IRequestHandler<ForgotPasswordCommand, Result>
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
        var tokenResult = await identityService.GeneratePasswordResetTokenAsync(user, cancellationToken);
        
        if (tokenResult.IsFailure)
            return Result.Success(); // Do not leak generation failures to the client either

        var encodedToken = HttpUtility.UrlEncode(tokenResult.Value);
        var encodedEmail = HttpUtility.UrlEncode(user.Email);
        var baseUrl = configuration["Frontend:BaseUrl"];
        
        var resetLink = $"{baseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";
        
        backgroundJobClient.Enqueue<IEmailService>(x => x.SendPasswordResetEmailAsync(
            user.Email!, 
            resetLink, 
            CancellationToken.None));

        return Result.Success();
    }
}
