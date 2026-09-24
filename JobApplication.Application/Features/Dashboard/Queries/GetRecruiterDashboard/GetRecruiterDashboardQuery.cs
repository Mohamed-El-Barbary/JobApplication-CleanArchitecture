using JobApplication.Application.DTOs.Dashboard;
using JobApplication.Domain.Common;
using MediatR;
using System;

namespace JobApplication.Application.Features.Dashboard.Queries.GetRecruiterDashboard;

public record GetRecruiterDashboardQuery(Guid UserId) : IRequest<Result<RecruiterDashboardResponse>>;
