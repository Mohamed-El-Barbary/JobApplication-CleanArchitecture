namespace JobApplication.Application.Features.Auth.Commands.RefreshToken;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Domain.Common;
using MediatR;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<AuthenticationResponse>>;
