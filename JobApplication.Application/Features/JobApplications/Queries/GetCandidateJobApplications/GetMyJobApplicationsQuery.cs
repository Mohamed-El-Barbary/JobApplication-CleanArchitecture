using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Queries.GetCandidateJobApplications;

public sealed record GetMyJobApplicationsQuery(Guid UserId) : IRequest<Result<IEnumerable<JobApplicationResponse>>>;