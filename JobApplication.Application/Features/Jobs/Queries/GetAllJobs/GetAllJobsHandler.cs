using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Queries.GetAllJobs;

public class GetAllJobsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllJobsQuery, Result<IEnumerable<JobResponse>>>
{
    public async Task<Result<IEnumerable<JobResponse>>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await unitOfWork.Repository<Job>().GetAllAsync(cancellationToken);
        return jobs
            .Where(j => j.Status == JobStatus.Open)
            .Adapt<List<JobResponse>>();
    }
}
