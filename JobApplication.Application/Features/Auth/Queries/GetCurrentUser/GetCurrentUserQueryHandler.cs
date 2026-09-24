namespace JobApplication.Application.Features.Auth.Queries.GetCurrentUser;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService) : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId == null)
            return Error.Unauthorized("Auth.GetCurrentUser.Unauthorized", "User is not authenticated.");

        var userResult = await identityService.GetUserByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (userResult.IsFailure)
            return userResult.Error!;

        var user = userResult.Value;
        var rolesResult = await identityService.GetRolesAsync(user, cancellationToken);
        var roles = rolesResult.IsSuccess ? rolesResult.Value : new System.Collections.Generic.List<string>();

        var response = new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            EmailConfirmed = await identityService.IsEmailConfirmedAsync(user, cancellationToken),
            Roles = roles
        };

        return response;
    }
}
