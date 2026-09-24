using JobApplication.Application.DTOs.Candidates;
using JobApplication.Domain.Common;
using MediatR;
using System;

namespace JobApplication.Application.Features.Candidates.Queries.GetMyCandidateProfile;

public record GetMyCandidateProfileQuery(Guid UserId) : IRequest<Result<CandidateProfileResponse>>;
