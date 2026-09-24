namespace JobApplication.Application.Features.Auth.Commands.RevokeRefreshToken;

using JobApplication.Domain.Common;
using MediatR;

public record RevokeRefreshTokenCommand(string RefreshToken) : IRequest<Result>;
