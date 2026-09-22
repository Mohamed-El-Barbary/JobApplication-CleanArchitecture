using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob;

public sealed record CloseJobCommand(
    int JobId,
    Guid UserId) : IRequest<Result>;
