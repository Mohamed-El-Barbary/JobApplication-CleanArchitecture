using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Commands.RescheduleInterview;

public sealed class RescheduleInterviewHandler(IUnitOfWork unitOfWork) : IRequestHandler<RescheduleInterviewCommand, Result>
{
    public async Task<Result> Handle(RescheduleInterviewCommand request, CancellationToken cancellationToken)
    {
        if (request.ScheduledAt <= DateTime.UtcNow)
            return Result.Failure(Error.Validation("ScheduledAt.Validation", "Rescheduled time must be in the future."));

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);
        if (recruiter is null)
            return Result.Failure(Error.NotFound("Recruiter.NotFound", "Recruiter was not found."));

        var interview = await unitOfWork.Repository<Interview>().GetByIdAsync(request.InterviewId, cancellationToken);
        if (interview is null)
            return Result.Failure(Error.NotFound("Interview.NotFound", $"Interview with ID '{request.InterviewId}' was not found."));

        var application = await unitOfWork.Repository<JobCandidateApplication>().GetByIdAsync(interview.JobApplicationId, cancellationToken);
        if (application is null)
            return Result.Failure(Error.NotFound("JobApplication.NotFound", $"Job application with ID '{interview.JobApplicationId}' was not found."));

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(application.JobId, cancellationToken);
        if (job is null)
            return Result.Failure(Error.NotFound("Job.NotFound", $"Job with ID '{application.JobId}' was not found."));

        if (job.RecruiterId != recruiter.Id)
            return Result.Failure(Error.Unauthorized("User.Unauthorized", "You are not authorized to manage this interview."));

        if (interview.Status == InterviewStatus.Cancelled || interview.Status == InterviewStatus.Completed)
            return Result.Failure(Error.Validation("Interview.Validation", "Cannot reschedule a cancelled or completed interview."));

        interview.ScheduledAt = request.ScheduledAt;
        interview.Status = InterviewStatus.Rescheduled;
        interview.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Interview>().Update(interview);
        
        var res = await unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        return res
            ? Result.Success()
            : Result.Failure(Error.Failure("SaveChanges.Failed", "Failed to save changes."));
    }
}
