using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Queries.GetMyJobs;

public class GetMyJobsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyJobsQuery, Result<IEnumerable<JobResponse>>>
{
    public async Task<Result<IEnumerable<JobResponse>>> Handle(GetMyJobsQuery request, CancellationToken cancellationToken)
    {
        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);

        if (recruiter is null)
            return Error.NotFound("Recruiter.NotFound", $"Recruiter with {request.UserId} was not found");

        var jobs = await unitOfWork.Repository<Job>().GetAllAsync(cancellationToken);

        return jobs
            .Where(j => j.RecruiterId == recruiter.Id)
            .Adapt<List<JobResponse>>();
    }
}
