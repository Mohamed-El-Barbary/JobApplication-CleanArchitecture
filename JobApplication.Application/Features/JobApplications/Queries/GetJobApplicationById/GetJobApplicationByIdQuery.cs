using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Queries.GetJobApplicationById;

public sealed record GetJobApplicationByIdQuery(
    int ApplicationId,
    Guid UserId) : IRequest<Result<JobApplicationResponse>>;