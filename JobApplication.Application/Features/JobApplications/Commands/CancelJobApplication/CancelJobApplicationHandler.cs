using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace JobApplication.Application.Features.JobApplications.Commands.CancelJobApplication;

public sealed class CancelJobApplicationHandler(IUnitOfWork unitOfWork) : IRequestHandler<CancelJobApplicationCommand, Result>
{
    public async Task<Result> Handle(CancelJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var candidates = await unitOfWork.Repository<Candidate>().GetAllAsync(cancellationToken);
        var candidate = candidates.FirstOrDefault(c => c.UserId == request.UserId);
        if (candidate == null)
            return Result.Failure(Error.NotFound("Candidate.NotFound", $"Candidate was not found."));

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var application = applications.FirstOrDefault(a => a.Id == request.ApplicationId);
        if (application == null)
            return Result.Failure(Error.NotFound(
                "Candidate.NotFound",
                $"Job application with ID '{request.ApplicationId}' was not found."));

        if (application.CandidateId != candidate.Id)
            Result.Failure(Error.Unauthorized("User.Unauthorized", "You are not authorized to cancel this application."));

        if (application.Status == ApplicationStatus.Accepted ||
            application.Status == ApplicationStatus.Rejected ||
            application.Status == ApplicationStatus.Cancelled)
            return Result.Failure(Error.Validation(
                "Application.Validation", 
                "Application cannot be cancelled in its current status."));

        application.Status = ApplicationStatus.Cancelled;

        unitOfWork.Repository<JobCandidateApplication>().Update(application);
        var res = await unitOfWork.SaveChangesAsync(cancellationToken) > 0;

        return res
            ? Result.Success()
            : Result.Failure(Error.Failure("SaveChanges.Failed", "Failed to save changes."));
    }
}
