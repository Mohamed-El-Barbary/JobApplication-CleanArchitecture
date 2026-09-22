using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Queries.GetMyJobs;

public sealed record GetMyJobsQuery(Guid UserId) : IRequest<Result<IEnumerable<JobResponse>>>;
