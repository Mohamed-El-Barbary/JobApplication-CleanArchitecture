using JobApplication.Application.DTOs.Candidates;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Candidates.Commands.DeleteCandidateCv;

internal class DeleteCandidateCvHandler(
    IUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService) : IRequestHandler<DeleteCandidateCvCommand, Result<CandidateProfileResponse>>
{
    public async Task<Result<CandidateProfileResponse>> Handle(DeleteCandidateCvCommand command, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.Repository<Candidate>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken);

        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", "Candidate profile not found.");

        if (string.IsNullOrEmpty(candidate.CVPublicId))
            return Error.Validation("Candidate.NoCv", "Candidate does not have a CV to delete.");

        // Delete from Cloudinary
        await cloudinaryService.DeleteAsync(candidate.CVPublicId, cancellationToken);

        // Update metadata
        candidate.CVPublicId = null;
        candidate.CVUrl = null!; // Keeping null! or string.Empty depending on if it's nullable. Let's make it null. Wait, CVUrl is not nullable in original?
        // Wait, original Candidate.cs: public string CVUrl { get; set; } = default!; It wasn't nullable.
        // Let's set it to string.Empty.
        candidate.CVUrl = string.Empty;
        candidate.CVFileName = null;
        candidate.CVUploadedAt = null;

        unitOfWork.Repository<Candidate>().Update(candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return candidate.Adapt<CandidateProfileResponse>();
    }
}
