using JobApplication.Application.DTOs.Candidates;
using JobApplication.Domain.Common;
using MediatR;
using System;

namespace JobApplication.Application.Features.Candidates.Commands.UpdateCandidateProfile;

public record UpdateCandidateProfileCommand(
    UpdateCandidateProfileRequest Request,
    Guid UserId) : IRequest<Result<CandidateProfileResponse>>;
