using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Queries.GetJobById;

public sealed record GetJobByIdQuery(int JobId) : IRequest<Result<JobResponse>>;

