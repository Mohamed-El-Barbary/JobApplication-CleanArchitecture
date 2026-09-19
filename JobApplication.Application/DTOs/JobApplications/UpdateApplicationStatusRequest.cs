using JobApplication.Domain.Enums;

namespace JobApplication.Application.DTOs.JobApplications;

public class UpdateApplicationStatusRequest
{
    public ApplicationStatus Status { get; set; }
}
