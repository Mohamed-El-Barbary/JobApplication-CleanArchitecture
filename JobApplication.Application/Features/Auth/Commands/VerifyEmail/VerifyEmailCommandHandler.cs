namespace JobApplication.Application.Features.Auth.Commands.VerifyEmail;

using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

internal class VerifyEmailCommandHandler(
    IIdentityService identityService) : IRequestHandler<VerifyEmailCommand, Result>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == System.Guid.Empty)
            return Result.Failure(Error.Validation("Auth.VerifyEmail.UserIdRequired", "UserId is required."));

        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure(Error.Validation("Auth.VerifyEmail.TokenRequired", "Token is required."));

        var userResult = await identityService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
            return Result.Failure(userResult.Error!);

        var decodedToken = HttpUtility.UrlDecode(request.Token).Replace(" ", "+");
        
        return await identityService.ConfirmEmailAsync(userResult.Value, decodedToken, cancellationToken);
    }
}
