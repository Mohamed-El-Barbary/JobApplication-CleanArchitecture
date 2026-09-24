namespace JobApplication.Application.Features.Auth.Commands.Login;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Domain.Common;
using MediatR;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthenticationResponse>>;
