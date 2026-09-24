namespace JobApplication.Application.Features.Auth.Commands.VerifyEmail;

using JobApplication.Domain.Common;
using MediatR;
using System;

public record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Result>;
