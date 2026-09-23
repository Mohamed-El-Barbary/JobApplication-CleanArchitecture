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

namespace JobApplication.Application.Features.JobApplications.Commands.UpdateJobApplication;

public sealed class UpdateJobApplicationHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateJobApplicationCommand, Result<JobApplicationResponse>>
{
    public async Task<Result<JobApplicationResponse>> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        if (request.YearsOfExperience.HasValue && request.YearsOfExperience.Value < 0)
            Error.Validation("YearsOfExperience.Validation", "YearsOfExperience cannot be negative.");

        var candidates = await unitOfWork.Repository<Candidate>().GetAllAsync(cancellationToken);
        var candidate = candidates.FirstOrDefault(c => c.UserId == request.UserId);
        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", $"Candidate was not found.");

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var application = applications.FirstOrDefault(a => a.Id == request.ApplicationId);
        if (application == null)
            return Error.NotFound("Application.NotFound", $"Job application with ID '{request.ApplicationId}' was not found.");

        if (application.CandidateId != candidate.Id)
            Error.Unauthorized("User.Unauthorized", "You are not authorized to update this application.");

        if (application.Status != ApplicationStatus.Applied)
            Error.Validation("Application.Validation", "Application can only be edited while in Applied status.");

        request.Adapt(application);

        unitOfWork.Repository<JobCandidateApplication>().Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return application.Adapt<JobApplicationResponse>();
    }
}
