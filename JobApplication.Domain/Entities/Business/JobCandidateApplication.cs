using JobApplication.Domain.Enums;
using System;

namespace JobApplication.Domain.Entities.Business;

public class JobCandidateApplication : BaseEntity
{
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;

    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    public string? CoverLetter { get; set; }
    public string? ResumeUrl { get; set; }
    public int? YearsOfExperience { get; set; }
    public ApplicationStatus Status { get; set; }

    public JobApplicationStatus ApplicationStatus
    {
        get => (JobApplicationStatus)Status;
        set => Status = (ApplicationStatus)value;
    }
}
