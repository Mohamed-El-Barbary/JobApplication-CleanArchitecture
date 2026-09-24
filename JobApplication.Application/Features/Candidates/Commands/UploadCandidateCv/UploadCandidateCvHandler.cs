using JobApplication.Application.DTOs.Candidates;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Candidates.Commands.UploadCandidateCv;

internal class UploadCandidateCvHandler(
    IUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService) : IRequestHandler<UploadCandidateCvCommand, Result<CandidateProfileResponse>>
{
    public async Task<Result<CandidateProfileResponse>> Handle(UploadCandidateCvCommand command, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.Repository<Candidate>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken);

        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", "Candidate profile not found.");

        // Validate file
        if (command.File == null || command.File.Length == 0)
            return Error.Validation("File.Empty", "File cannot be empty.");

        if (command.File.Length > 5 * 1024 * 1024) // 5MB
            return Error.Validation("File.Size", "File size cannot exceed 5MB.");

        var extension = Path.GetExtension(command.File.FileName).ToLowerInvariant();
        if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
            return Error.Validation("File.Type", "Only PDF, DOC, and DOCX files are allowed.");

        // Upload to Cloudinary
        var uploadResult = await cloudinaryService.UploadCvAsync(command.File, candidate.Id.ToString(), cancellationToken);

        var oldPublicId = candidate.CVPublicId;

        // Update candidate
        candidate.CVPublicId = uploadResult.PublicId;
        candidate.CVUrl = uploadResult.Url;
        candidate.CVFileName = uploadResult.FileName;
        candidate.CVUploadedAt = DateTime.UtcNow;

        unitOfWork.Repository<Candidate>().Update(candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Delete old CV if exists
        if (!string.IsNullOrEmpty(oldPublicId) && oldPublicId != uploadResult.PublicId)
        {
            try
            {
                await cloudinaryService.DeleteAsync(oldPublicId, cancellationToken);
            }
            catch
            {
                // Log exception if possible, but do not fail the upload
            }
        }

        return candidate.Adapt<CandidateProfileResponse>();
    }
}
