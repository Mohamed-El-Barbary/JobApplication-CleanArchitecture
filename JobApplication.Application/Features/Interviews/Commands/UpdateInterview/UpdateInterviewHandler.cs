using JobApplication.Application.DTOs.Interviews;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Commands.UpdateInterview;

public sealed class UpdateInterviewHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateInterviewCommand, Result<InterviewResponse>>
{
    public async Task<Result<InterviewResponse>> Handle(UpdateInterviewCommand request, CancellationToken cancellationToken)
    {
        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);
        if (recruiter is null)
            return Error.NotFound("Recruiter.NotFound", "Recruiter was not found.");

        var interview = await unitOfWork.Repository<Interview>().GetByIdAsync(request.InterviewId, cancellationToken);
        if (interview is null)
            return Error.NotFound("Interview.NotFound", $"Interview with ID '{request.InterviewId}' was not found.");

        var application = await unitOfWork.Repository<JobCandidateApplication>().GetByIdAsync(interview.JobApplicationId, cancellationToken);
        if (application is null)
            return Error.NotFound("JobApplication.NotFound", $"Job application with ID '{interview.JobApplicationId}' was not found.");

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(application.JobId, cancellationToken);
        if (job is null)
            return Error.NotFound("Job.NotFound", $"Job with ID '{application.JobId}' was not found.");

        if (job.RecruiterId != recruiter.Id)
            return Error.Unauthorized("User.Unauthorized", "You are not authorized to manage this interview.");

        if (interview.Status == InterviewStatus.Cancelled || interview.Status == InterviewStatus.Completed)
            return Error.Validation("Interview.Validation", "Cannot update a cancelled or completed interview.");

        request.Adapt(interview);
        interview.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Interview>().Update(interview);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return interview.Adapt<InterviewResponse>();
    }
}
