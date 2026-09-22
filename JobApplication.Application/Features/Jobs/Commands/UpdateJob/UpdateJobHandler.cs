using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.UpdateJob;

public sealed class UpdateJobHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateJobCommand, Result<JobResponse>>
{
    public async Task<Result<JobResponse>> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Error.Validation("Titile.NotFound", "Title is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Error.Validation("Description.NotFound", "Description is required.");

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
            return Error.NotFound("Job.NotFound", $"Job with ID '{request.JobId}' was not found.");


        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);
        if (recruiter is null)
            return Error.NotFound("Recruiter.NotFound", "Recuiter Was Not Found");

        job.Title = request!.Title.Trim();
        job.Description = request.Description.Trim();
        job.EmploymentType = request.EmploymentType;

        unitOfWork.Repository<Job>().Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return job.Adapt<JobResponse>();
    }
}
