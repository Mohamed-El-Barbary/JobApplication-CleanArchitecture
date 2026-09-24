namespace JobApplication.Application.DTOs.Dashboard;

public class CandidateDashboardResponse
{
    public int TotalApplications { get; set; }
    public int AppliedApplications { get; set; }
    public int UnderReviewApplications { get; set; }
    public int InterviewApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int CancelledApplications { get; set; }
    public int UpcomingInterviews { get; set; }
}
