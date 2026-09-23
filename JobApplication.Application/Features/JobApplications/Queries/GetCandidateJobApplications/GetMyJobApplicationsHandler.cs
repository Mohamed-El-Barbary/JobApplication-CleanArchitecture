using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Queries.GetCandidateJobApplications;

internal class GetMyJobApplicationsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyJobApplicationsQuery, Result<IEnumerable<JobApplicationResponse>>>
{
    public async Task<Result<IEnumerable<JobApplicationResponse>>> Handle(GetMyJobApplicationsQuery request, CancellationToken cancellationToken)
    {

        var candidates = await unitOfWork.Repository<Candidate>().GetAllAsync(cancellationToken);
        var candidate = candidates.FirstOrDefault(c => c.UserId == request.UserId);
        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", $"Candidate was not found.");

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        return applications
            .Where(a => a.CandidateId == candidate.Id)
            .Adapt<List<JobApplicationResponse>>();
    }
}
