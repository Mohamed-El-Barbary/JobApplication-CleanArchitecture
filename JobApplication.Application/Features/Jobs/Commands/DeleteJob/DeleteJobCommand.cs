using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.DeleteJob;

public sealed record DeleteJobCommand(
    int JobId, 
    Guid UserId) : IRequest<Result>;
