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

namespace JobApplication.Application.Features.Dashboard.Queries.GetRecruiterDashboard;

internal class GetRecruiterDashboardHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRecruiterDashboardQuery, Result<RecruiterDashboardResponse>>
{
    public async Task<Result<RecruiterDashboardResponse>> Handle(GetRecruiterDashboardQuery request, CancellationToken cancellationToken)
    {
        var recruiter = await unitOfWork.Repository<Recruiter>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == request.UserId, cancellationToken);

        if (recruiter == null)
            return Error.NotFound("Recruiter.NotFound", "Recruiter not found.");

        var recruiterId = recruiter.Id;

        // Job Stats
        var jobsStats = await unitOfWork.Repository<Job>()
            .GetQueryable()
            .AsNoTracking()
            .Where(j => j.RecruiterId == recruiterId)
            .GroupBy(j => j.IsActive)
            .Select(g => new { IsActive = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalJobs = jobsStats.Sum(x => x.Count);
        var openJobs = jobsStats.FirstOrDefault(x => x.IsActive)?.Count ?? 0;
        var closedJobs = jobsStats.FirstOrDefault(x => !x.IsActive)?.Count ?? 0;

        // Application Stats for Recruiter's Jobs
        var applicationStats = await unitOfWork.Repository<JobCandidateApplication>()
            .GetQueryable()
            .AsNoTracking()
            .Where(a => a.Job.RecruiterId == recruiterId)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalApplications = applicationStats.Sum(x => x.Count);
        
        var getCount = (ApplicationStatus status) => applicationStats.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

        var pendingApplications = getCount(ApplicationStatus.Applied) + getCount(ApplicationStatus.UnderReview);
        var interviewApplications = getCount(ApplicationStatus.Interview);
        var acceptedApplications = getCount(ApplicationStatus.Accepted);
        var rejectedApplications = getCount(ApplicationStatus.Rejected);

        // Count upcoming interviews for Recruiter's jobs
        var upcomingInterviews = await unitOfWork.Repository<Interview>()
            .GetQueryable()
            .AsNoTracking()
            .Where(i => i.JobApplication.Job.RecruiterId == recruiterId && i.Status == InterviewStatus.Scheduled && i.ScheduledAt >= DateTime.UtcNow)
            .CountAsync(cancellationToken);

        return new RecruiterDashboardResponse
        {
            TotalJobs = totalJobs,
            OpenJobs = openJobs,
            ClosedJobs = closedJobs,
            TotalApplications = totalApplications,
            PendingApplications = pendingApplications,
            InterviewCount = interviewApplications,
            AcceptedApplications = acceptedApplications,
            RejectedApplications = rejectedApplications,
            UpcomingInterviews = upcomingInterviews
        };
    }
}
