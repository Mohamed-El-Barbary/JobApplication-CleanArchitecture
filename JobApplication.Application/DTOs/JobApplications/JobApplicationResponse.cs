using JobApplication.Domain.Enums;
using System;

namespace JobApplication.Application.DTOs.JobApplications;

public class JobApplicationResponse
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public int JobId { get; set; }

    public string? CoverLetter { get; set; }

    public string? ResumeUrl { get; set; }

    public int? YearsOfExperience { get; set; }

    public ApplicationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
