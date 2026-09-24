using JobApplication.Application.DTOs.Dashboard;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Dashboard.Queries.GetCandidateDashboard;

internal class GetCandidateDashboardHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCandidateDashboardQuery, Result<CandidateDashboardResponse>>
{
    public async Task<Result<CandidateDashboardResponse>> Handle(GetCandidateDashboardQuery request, CancellationToken cancellationToken)
    {
        var candidate = await unitOfWork.Repository<Candidate>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (candidate == null)
            return Error.NotFound("Candidate.NotFound", "Candidate not found.");

        var candidateId = candidate.Id;

        var applications = await unitOfWork.Repository<JobCandidateApplication>()
            .GetQueryable()
            .AsNoTracking()
            .Where(a => a.CandidateId == candidateId)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalApplications = applications.Sum(x => x.Count);
        
        var getCount = (ApplicationStatus status) => applications.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

        // Count upcoming interviews (Status = Scheduled and Date >= Now)
        var upcomingInterviews = await unitOfWork.Repository<Interview>()
            .GetQueryable()
            .AsNoTracking()
            .Where(i => i.JobApplication.CandidateId == candidateId && i.Status == InterviewStatus.Scheduled && i.ScheduledAt >= DateTime.UtcNow)
            .CountAsync(cancellationToken);

        return new CandidateDashboardResponse
        {
            TotalApplications = totalApplications,
            AppliedApplications = getCount(ApplicationStatus.Applied),
            UnderReviewApplications = getCount(ApplicationStatus.UnderReview),
            InterviewApplications = getCount(ApplicationStatus.Interview),
            AcceptedApplications = getCount(ApplicationStatus.Accepted),
            RejectedApplications = getCount(ApplicationStatus.Rejected),
            CancelledApplications = getCount(ApplicationStatus.Cancelled),
            UpcomingInterviews = upcomingInterviews
        };
    }
}
