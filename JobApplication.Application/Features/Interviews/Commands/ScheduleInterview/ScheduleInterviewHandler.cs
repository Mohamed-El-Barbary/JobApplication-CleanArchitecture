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

namespace JobApplication.Application.Features.Interviews.Commands.ScheduleInterview;

public sealed class ScheduleInterviewHandler(IUnitOfWork unitOfWork) : IRequestHandler<ScheduleInterviewCommand, Result<InterviewResponse>>
{
    public async Task<Result<InterviewResponse>> Handle(ScheduleInterviewCommand request, CancellationToken cancellationToken)
    {
        if (request.ScheduledAt <= DateTime.UtcNow)
            return Error.Validation("ScheduledAt.Validation", "Interview must be scheduled in the future.");

        if (request.Duration <= TimeSpan.Zero)
            return Error.Validation("Duration.Validation", "Duration must be positive.");

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);
        if (recruiter is null)
            return Error.NotFound("Recruiter.NotFound", "Recruiter was not found.");

        var application = await unitOfWork.Repository<JobCandidateApplication>().GetByIdAsync(request.ApplicationId, cancellationToken);
        if (application is null)
            return Error.NotFound("JobApplication.NotFound", $"Job application with ID '{request.ApplicationId}' was not found.");

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(application.JobId, cancellationToken);
        if (job is null)
            return Error.NotFound("Job.NotFound", $"Job with ID '{application.JobId}' was not found.");

        if (job.RecruiterId != recruiter.Id)
            return Error.Unauthorized("User.Unauthorized", "You are not authorized to manage this application.");

        if (application.Status == ApplicationStatus.Cancelled ||
            application.Status == ApplicationStatus.Rejected)
            return Error.Validation("Application.Validation", "Cannot schedule an interview for a cancelled or rejected application.");

        if (application.Status != ApplicationStatus.UnderReview &&
            application.Status != ApplicationStatus.Interview)
            return Error.Validation("Application.Validation", "Application must be in UnderReview or Interview status to schedule an interview.");

        var interview = request.Adapt<Interview>();
        interview.JobApplicationId = request.ApplicationId;
        interview.Status = InterviewStatus.Scheduled;

        await unitOfWork.Repository<Interview>().AddAsync(interview);

        if (application.Status == ApplicationStatus.UnderReview)
        {
            application.Status = ApplicationStatus.Interview;
            unitOfWork.Repository<JobCandidateApplication>().Update(application);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return interview.Adapt<InterviewResponse>();
    }
}
