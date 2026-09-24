namespace JobApplication.Application.DTOs.Dashboard;

public class RecruiterDashboardResponse
{
    public int TotalJobs { get; set; }
    public int OpenJobs { get; set; }
    public int ClosedJobs { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; } // Applied + UnderReview
    public int InterviewCount { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int UpcomingInterviews { get; set; }
}
