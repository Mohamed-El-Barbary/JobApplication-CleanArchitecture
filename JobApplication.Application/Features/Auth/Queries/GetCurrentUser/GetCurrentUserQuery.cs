namespace JobApplication.Application.Features.Auth.Queries.GetCurrentUser;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Domain.Common;
using MediatR;

public record GetCurrentUserQuery : IRequest<Result<CurrentUserResponse>>;
