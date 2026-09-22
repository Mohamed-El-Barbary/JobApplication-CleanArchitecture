using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob;

public sealed class CloseJobHandler(IUnitOfWork unitOfWork) : IRequestHandler<CloseJobCommand, Result>
{
    public async Task<Result> Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await unitOfWork.Repository<Job>().GetByIdAsync(request.JobId, cancellationToken);

        if (job == null)
            Error.NotFound("Job.NotFound", $"Job with ID '{request.JobId}' was not found.");

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == request.UserId);

        if (recruiter is null)
            return Result.Failure(Error.NotFound("Recruiter.NotFound", "Recuiter Was Not Found"));


        job!.Status = JobStatus.Closed;
        job.ClosedAt = DateTime.UtcNow;

        unitOfWork.Repository<Job>().Update(job);
        var res = await unitOfWork.SaveChangesAsync(cancellationToken) > 0;

        return res
            ? Result.Success()
            : Result.Failure(
                Error.Failure("SaveChanges.Failed", "Failed to save changes."));
    }
}
