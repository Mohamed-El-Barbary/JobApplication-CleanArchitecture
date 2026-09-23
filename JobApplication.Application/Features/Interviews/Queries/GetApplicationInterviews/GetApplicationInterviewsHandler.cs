using JobApplication.Application.DTOs.Interviews;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Queries.GetApplicationInterviews;

public sealed class GetApplicationInterviewsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetApplicationInterviewsQuery, Result<IEnumerable<InterviewResponse>>>
{
    public async Task<Result<IEnumerable<InterviewResponse>>> Handle(GetApplicationInterviewsQuery request, CancellationToken cancellationToken)
    {
        var application = await unitOfWork.Repository<JobCandidateApplication>().GetByIdAsync(request.ApplicationId, cancellationToken);
        if (application is null)
            return Error.NotFound("JobApplication.NotFound", $"Job application with ID '{request.ApplicationId}' was not found.");

        var candidate = await FindCandidateByUserIdAsync(request.UserId, cancellationToken);
        var recruiter = await FindRecruiterByUserIdAsync(request.UserId, cancellationToken);

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(application.JobId, cancellationToken);

        var isCandidateOwner = candidate is not null && application.CandidateId == candidate.Id;
        var isRecruiterOwner = recruiter is not null && job is not null && job.RecruiterId == recruiter.Id;

        if (!isCandidateOwner && !isRecruiterOwner)
            return Error.Unauthorized("User.Unauthorized", "You are not authorized to view interviews for this application.");

        var interviews = await unitOfWork.Repository<Interview>().GetAllAsync(cancellationToken);
        
        var applicationInterviews = interviews
            .Where(i => i.JobApplicationId == request.ApplicationId)
            .OrderBy(i => i.ScheduledAt)
            .Adapt<List<InterviewResponse>>();

        return applicationInterviews;
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
