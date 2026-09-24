using JobApplication.Application.DTOs.Candidates;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Candidates.Queries.GetMyCandidateProfile;

internal class GetMyCandidateProfileHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyCandidateProfileQuery, Result<CandidateProfileResponse>>
{
    public async Task<Result<CandidateProfileResponse>> Handle(GetMyCandidateProfileQuery request, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.Repository<Candidate>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", "Candidate profile not found.");

        return candidate.Adapt<CandidateProfileResponse>();
    }
}
