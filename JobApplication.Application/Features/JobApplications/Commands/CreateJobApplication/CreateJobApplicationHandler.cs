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

namespace JobApplication.Application.Features.JobApplications.Commands.CreateJobApplication;

public sealed class CreateJobApplicationHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateJobApplicationCommand, Result<JobApplicationResponse>>
{
    public async Task<Result<JobApplicationResponse>> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        if (request.YearsOfExperience.HasValue && request.YearsOfExperience.Value < 0)
            Error.Validation("YearsOfExperience.Validation", "YearsOfExperience cannot be negative.");

        var candidates = await unitOfWork.Repository<Candidate>().GetAllAsync(cancellationToken);
        var candidate = candidates.FirstOrDefault(c => c.UserId == request.UserId);
        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", $"Candidate was not found.");

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
            return Error.NotFound("Candidate.NotFound", $"Job with ID '{request.JobId}' was not found.");

        if (job.Status != JobStatus.Open)
            return Error.Validation("Job.Validation", "Cannot apply to a job that is not open.");

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var existingApplication = applications.FirstOrDefault(a => a.CandidateId == candidate.Id && a.JobId == request.JobId);
        if (existingApplication != null)
            Error.Validation("Application.Validation", "Candidate has already applied to this job.");


        var application = request.Adapt<JobCandidateApplication>();
        application.CandidateId = candidate.Id;
        application.JobId = request.JobId;

        await unitOfWork.Repository<JobCandidateApplication>().AddAsync(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return application.Adapt<JobApplicationResponse>();
    }
}
