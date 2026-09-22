using JobApplication.Application.DTOs.Jobs;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.CreateJob;

public class CreateJobHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateJobCommand, Result<JobResponse>>
{
    public async Task<Result<JobResponse>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Error.Validation("Titile.NotFound", "Title is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Error.Validation("Description.NotFound", "Description is required.");

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);

        if (recruiter is null)
            return Error.NotFound("Recruiter.NotFound", "Recuiter Was Not Found");

        var job = request.Adapt<Job>();

        job.RecruiterId = recruiter.Id;

        await unitOfWork.Repository<Job>().AddAsync(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return job.Adapt<JobResponse>();
    }
}
