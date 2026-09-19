namespace JobApplication.Application.DTOs.JobApplications;

public class CreateJobApplicationRequest
{
    public string? CoverLetter { get; set; }

    public string? ResumeUrl { get; set; }

    public int? YearsOfExperience { get; set; }
}
