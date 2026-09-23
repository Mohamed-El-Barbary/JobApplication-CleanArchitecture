using JobApplication.Domain.Enums;

namespace JobApplication.Application.DTOs.Interviews;

public class UpdateInterviewRequest
{
    public DateTime ScheduledAt { get; set; }

    public TimeSpan Duration { get; set; }

    public InterviewType Type { get; set; }

    public string? MeetingUrl { get; set; }

    public string? Notes { get; set; }
}
