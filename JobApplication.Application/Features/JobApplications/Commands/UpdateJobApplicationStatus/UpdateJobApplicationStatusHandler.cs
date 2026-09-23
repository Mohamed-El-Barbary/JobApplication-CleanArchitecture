using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace JobApplication.Application.Features.JobApplications.Commands.UpdateJobApplicationStatus;

public sealed class UpdateJobApplicationStatusHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateJobApplicationStatusCommand, Result<JobApplicationResponse>>
{
    public async Task<Result<JobApplicationResponse>> Handle(UpdateJobApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.Status == ApplicationStatus.Cancelled)
            return Error.Validation("Status.Validation", "Recruiters cannot set application status to Cancelled.");

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);
        if (recruiter is null)
            return Error.NotFound("Recruiter.NotFound", $"Recruiter with ID '{request.UserId}' was not found.");

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var application = applications.FirstOrDefault(a => a.Id == request.ApplicationId);
        if (application is null)
            return Error.NotFound("JobApplication.NotFound", $"Job application with ID '{request.ApplicationId}' was not found.");


        var job = await unitOfWork.Repository<Job>().GetByIdAsync(application.JobId, cancellationToken);
        if (job is null)
            return Error.NotFound("Job.NotFound", $"Job with ID '{application.JobId}' was not found.");

        if (job.RecruiterId != recruiter.Id)
            return Error.Unauthorized("User.Unauthorized", "You are not authorized to update this application.");

        var transitionError = ValidateStatusTransition(application.Status, request.Status);
        if (transitionError is not null)
            return transitionError;

        application.Status = request.Status;

        unitOfWork.Repository<JobCandidateApplication>().Update(application);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return application.Adapt<JobApplicationResponse>();
    }

    private static Error? ValidateStatusTransition(ApplicationStatus current, ApplicationStatus target)
    {
        if (current == ApplicationStatus.Cancelled)
            return Error.Validation(
                "Status.Validation",
                "Cannot update the status of a cancelled application.");

        if (current == ApplicationStatus.Accepted &&
            target is ApplicationStatus.Interview or ApplicationStatus.Applied)
            return Error.Validation(
                "Status.Validation",
                $"Invalid status transition from {current} to {target}.");

        if (current == ApplicationStatus.Rejected &&
            target is ApplicationStatus.Applied or ApplicationStatus.Interview)
            return Error.Validation(
                "Status.Validation",
                $"Invalid status transition from {current} to {target}.");

        return null;
    }
}
