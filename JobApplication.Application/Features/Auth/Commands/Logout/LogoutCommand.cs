namespace JobApplication.Application.Features.Auth.Commands.Logout;

using JobApplication.Domain.Common;
using MediatR;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
