using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Queries.GetRecruiterJobApplications;

public sealed record GetRecruiterJobApplicationsQuery(
    int JobId,
    Guid UserId) : IRequest<Result<IEnumerable<JobApplicationResponse>>>;