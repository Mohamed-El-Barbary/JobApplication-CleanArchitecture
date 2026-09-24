namespace JobApplication.Application.Features.Auth.Commands.Register;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Domain.Common;
using MediatR;

public record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string Role
) : IRequest<Result<AuthenticationResponse>>;
