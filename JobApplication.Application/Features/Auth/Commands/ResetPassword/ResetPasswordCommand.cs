namespace JobApplication.Application.Features.Auth.Commands.ResetPassword;

using JobApplication.Domain.Common;
using MediatR;

public record ResetPasswordCommand(string Email, string Token, string NewPassword, string ConfirmNewPassword) : IRequest<Result>;
