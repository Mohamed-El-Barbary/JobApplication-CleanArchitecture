using JobApplication.Application.DTOs.Interviews;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Queries.GetInterviewById;

public sealed class GetInterviewByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetInterviewByIdQuery, Result<InterviewResponse>>
{
    public async Task<Result<InterviewResponse>> Handle(GetInterviewByIdQuery request, CancellationToken cancellationToken)
    {
        var interview = await unitOfWork.Repository<Interview>().GetByIdAsync(request.InterviewId, cancellationToken);
        if (interview is null)
            return Error.NotFound("Interview.NotFound", $"Interview with ID '{request.InterviewId}' was not found.");

        var application = await unitOfWork.Repository<JobCandidateApplication>().GetByIdAsync(interview.JobApplicationId, cancellationToken);
        if (application is null)
            return Error.NotFound("JobApplication.NotFound", "Associated job application was not found.");

        var candidate = await FindCandidateByUserIdAsync(request.UserId, cancellationToken);
        var recruiter = await FindRecruiterByUserIdAsync(request.UserId, cancellationToken);

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(application.JobId, cancellationToken);

        var isCandidateOwner = candidate is not null && application.CandidateId == candidate.Id;
        var isRecruiterOwner = recruiter is not null && job is not null && job.RecruiterId == recruiter.Id;

        if (!isCandidateOwner && !isRecruiterOwner)
            return Error.Unauthorized("User.Unauthorized", "You are not authorized to view this interview.");

        return interview.Adapt<InterviewResponse>();
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
