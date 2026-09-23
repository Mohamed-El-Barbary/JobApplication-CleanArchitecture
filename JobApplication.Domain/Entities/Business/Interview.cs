using JobApplication.Domain.Enums;

namespace JobApplication.Domain.Entities.Business;

public class Interview : BaseEntity
{
    public int JobApplicationId { get; set; }
    public JobCandidateApplication JobApplication { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }
    public TimeSpan Duration { get; set; }
    public InterviewType Type { get; set; }
    public string? MeetingUrl { get; set; }
    public InterviewStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
