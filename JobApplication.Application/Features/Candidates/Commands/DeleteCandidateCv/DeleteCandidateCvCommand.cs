using JobApplication.Application.DTOs.Candidates;
using JobApplication.Domain.Common;
using MediatR;
using System;

namespace JobApplication.Application.Features.Candidates.Commands.DeleteCandidateCv;

public record DeleteCandidateCvCommand(Guid UserId) : IRequest<Result<CandidateProfileResponse>>;
