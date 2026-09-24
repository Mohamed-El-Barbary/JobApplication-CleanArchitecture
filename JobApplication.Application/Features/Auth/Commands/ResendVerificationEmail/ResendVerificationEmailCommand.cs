namespace JobApplication.Application.Features.Auth.Commands.ResendVerificationEmail;

using JobApplication.Domain.Common;
using MediatR;

public record ResendVerificationEmailCommand(string Email) : IRequest<Result>;
