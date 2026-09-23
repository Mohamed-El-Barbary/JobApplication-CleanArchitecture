using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Queries.GetRecruiterJobApplications;

public sealed class GetRecruiterJobApplicationsHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetRecruiterJobApplicationsQuery, Result<IEnumerable<JobApplicationResponse>>>
{
    public async Task<Result<IEnumerable<JobApplicationResponse>>> Handle(GetRecruiterJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);

        if (recruiter is null)
            return Error.NotFound("Recruiter.NotFound", $"Recruiter with {request.UserId} was not found");

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
            Error.NotFound("Job.NotFound", $"Job with ID '{request.JobId}' was not found.");

        if (job!.RecruiterId != recruiter.Id)
            return Error.Unauthorized("User.Unauthorized", "You are not authorized to view applications for this job.");

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);

        return applications
            .Where(a => a.JobId == request.JobId)
            .Adapt<List<JobApplicationResponse>>();
    }
}
