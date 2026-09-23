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

namespace JobApplication.Application.Features.Interviews.Queries.GetMyInterviews;

public sealed class GetMyInterviewsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyInterviewsQuery, Result<IEnumerable<InterviewResponse>>>
{
    public async Task<Result<IEnumerable<InterviewResponse>>> Handle(GetMyInterviewsQuery request, CancellationToken cancellationToken)
    {
        var candidate = await FindCandidateByUserIdAsync(request.UserId, cancellationToken);
        var recruiter = await FindRecruiterByUserIdAsync(request.UserId, cancellationToken);

        if (candidate is null && recruiter is null)
            return Error.NotFound("User.NotFound", "User profile not found.");

        var interviews = await unitOfWork.Repository<Interview>().GetAllAsync(cancellationToken);
        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var jobs = await unitOfWork.Repository<Job>().GetAllAsync(cancellationToken);

        IEnumerable<Interview> userInterviews = new List<Interview>();

        if (candidate is not null)
        {
            var candidateAppIds = applications
                .Where(a => a.CandidateId == candidate.Id)
                .Select(a => a.Id)
                .ToHashSet();

            userInterviews = interviews.Where(i => candidateAppIds.Contains(i.JobApplicationId));
        }
        else if (recruiter is not null)
        {
            var recruiterJobIds = jobs
                .Where(j => j.RecruiterId == recruiter.Id)
                .Select(j => j.Id)
                .ToHashSet();

            var recruiterAppIds = applications
                .Where(a => recruiterJobIds.Contains(a.JobId))
                .Select(a => a.Id)
                .ToHashSet();

            userInterviews = interviews.Where(i => recruiterAppIds.Contains(i.JobApplicationId));
        }

        return userInterviews
            .OrderBy(i => i.ScheduledAt)
            .Adapt<List<InterviewResponse>>();
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
