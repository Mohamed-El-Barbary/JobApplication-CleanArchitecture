using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Commands.CancelJobApplication;

public sealed record CancelJobApplicationCommand(
    int ApplicationId,
    Guid UserId) : IRequest<Result>;