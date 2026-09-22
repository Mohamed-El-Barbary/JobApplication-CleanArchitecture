using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Queries.GetJobById;

public class GetJobByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetJobByIdQuery, Result<JobResponse>>
{
    public async Task<Result<JobResponse>> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await unitOfWork.Repository<Job>().GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
            return Error.NotFound("Job.NotFound",$"Job with ID '{request.JobId}' was not found.");

        return job.Adapt<JobResponse>();
    }
}
