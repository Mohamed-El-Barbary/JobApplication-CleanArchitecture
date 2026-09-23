using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using JobApplication.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Commands.UpdateJobApplicationStatus;

public sealed record UpdateJobApplicationStatusCommand(
    ApplicationStatus Status,
    int ApplicationId,
    Guid UserId) : IRequest<Result<JobApplicationResponse>>;