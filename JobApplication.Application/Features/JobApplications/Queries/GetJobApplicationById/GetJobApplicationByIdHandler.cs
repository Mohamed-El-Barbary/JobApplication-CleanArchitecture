using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace JobApplication.Application.Features.JobApplications.Queries.GetJobApplicationById;

public sealed class GetJobApplicationByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetJobApplicationByIdQuery, Result<JobApplicationResponse>>
{
    public async Task<Result<JobApplicationResponse>> Handle(GetJobApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);

        var application = applications.FirstOrDefault(a => a.Id == request.ApplicationId);

        if (application is null)
            return Error.NotFound("Application.NotFound", $"Job application with ID '{request.ApplicationId}' was not found.");

        var candidate = await FindCandidateByUserIdAsync(request.UserId, cancellationToken);

        var recruiter = await FindRecruiterByUserIdAsync(request.UserId, cancellationToken);

        var jobs = await unitOfWork.Repository<Job>().GetAllAsync(cancellationToken);

        var job = jobs.FirstOrDefault(j => j.Id == application.JobId);

        var isCandidateOwner =
            candidate is not null &&
            application.CandidateId == candidate.Id;

        var isRecruiterOwner =
            recruiter is not null &&
            job is not null &&
            job.RecruiterId == recruiter.Id;

        if (!isCandidateOwner && !isRecruiterOwner)
            return Error.Unauthorized("User.Unauthorized", "You are not authorized to view this application.");


        return application.Adapt<JobApplicationResponse>();
    }

    private async Task<Candidate?> FindCandidateByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var candidates = await unitOfWork.Repository<Candidate>().GetAllAsync(cancellationToken);

        return candidates.FirstOrDefault(c => c.UserId == userId);
    }

    private async Task<Recruiter?> FindRecruiterByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);

        return recruiters.FirstOrDefault(r => r.UserId == userId);
    }
}
