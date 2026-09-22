using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using JobApplication.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.UpdateJob;

public sealed record UpdateJobCommand(
    string Title,
    string Description,
    EmploymentType EmploymentType,
    int JobId,
    Guid UserId) : IRequest<Result<JobResponse>>;