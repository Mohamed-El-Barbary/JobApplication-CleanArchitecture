namespace JobApplication.Application.Features.Auth.Commands.ResetPassword;

using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

internal class ResetPasswordCommandHandler(
    IIdentityService identityService) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure(Error.Validation("Auth.ResetPassword.EmailRequired", "Email is required."));
            
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure(Error.Validation("Auth.ResetPassword.TokenRequired", "Token is required."));

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return Result.Failure(Error.Validation("Auth.ResetPassword.NewPasswordRequired", "New password is required."));

        if (request.NewPassword != request.ConfirmNewPassword)
            return Result.Failure(Error.Validation("Auth.ResetPassword.PasswordMismatch", "New passwords do not match."));

        var userResult = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userResult.IsFailure)
            return Result.Failure(userResult.Error!); // Or return a generic error if desired, but user already has token from email

        // The token needs to be decoded before using it with Identity
        // Depending on how it's sent from the client, we might need to HttpUtility.UrlDecode it, 
        // but if the client already sends it decoded we don't. Let's assume the client passes the raw string from URL.
        var decodedToken = HttpUtility.UrlDecode(request.Token);
        // Replace spaces with + if they got stripped, though UrlDecode usually handles it.
        decodedToken = decodedToken.Replace(" ", "+");

        var resetResult = await identityService.ResetPasswordAsync(userResult.Value, decodedToken, request.NewPassword, cancellationToken);
        if (resetResult.IsFailure)
            return Result.Failure(resetResult.Error!);

        await identityService.RevokeAllRefreshTokensAsync(userResult.Value, cancellationToken);

        return Result.Success();
    }
}
