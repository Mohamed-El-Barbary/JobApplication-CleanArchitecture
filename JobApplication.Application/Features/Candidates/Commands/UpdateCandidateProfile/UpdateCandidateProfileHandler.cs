using JobApplication.Application.DTOs.Candidates;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Candidates.Commands.UpdateCandidateProfile;

internal class UpdateCandidateProfileHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateCandidateProfileCommand, Result<CandidateProfileResponse>>
{
    public async Task<Result<CandidateProfileResponse>> Handle(UpdateCandidateProfileCommand command, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.Repository<Candidate>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken);

        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", "Candidate profile not found.");

        command.Request.Adapt(candidate);
        
        unitOfWork.Repository<Candidate>().Update(candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return candidate.Adapt<CandidateProfileResponse>();
    }
}
