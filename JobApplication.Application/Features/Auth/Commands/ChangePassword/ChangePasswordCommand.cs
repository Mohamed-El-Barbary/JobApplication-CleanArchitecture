namespace JobApplication.Application.Features.Auth.Commands.ChangePassword;

using JobApplication.Domain.Common;
using MediatR;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmNewPassword) : IRequest<Result>;
