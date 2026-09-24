namespace JobApplication.Application.Features.Auth.Commands.ForgotPassword;

using JobApplication.Domain.Common;
using MediatR;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
