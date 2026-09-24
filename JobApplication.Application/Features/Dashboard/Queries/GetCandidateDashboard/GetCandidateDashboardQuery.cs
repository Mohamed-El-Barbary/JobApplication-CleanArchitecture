using JobApplication.Application.DTOs.Dashboard;
using JobApplication.Domain.Common;
using MediatR;
using System;

namespace JobApplication.Application.Features.Dashboard.Queries.GetCandidateDashboard;

public record GetCandidateDashboardQuery(Guid UserId) : IRequest<Result<CandidateDashboardResponse>>;
