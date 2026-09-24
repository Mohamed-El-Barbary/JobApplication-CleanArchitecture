using JobApplication.Application.DTOs.Candidates;
using JobApplication.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace JobApplication.Application.Features.Candidates.Commands.UploadCandidateCv;

public record UploadCandidateCvCommand(
    IFormFile File,
    Guid UserId) : IRequest<Result<CandidateProfileResponse>>;
