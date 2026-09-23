using JobApplication.Domain.Enums;

namespace JobApplication.Application.DTOs.Interviews;

public class InterviewResponse
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public TimeSpan Duration { get; set; }

    public InterviewType Type { get; set; }

    public string? MeetingUrl { get; set; }

    public InterviewStatus Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
