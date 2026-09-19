namespace JobApplication.Application.DTOs.Jobs;

using JobApplication.Domain.Enums;

public class UpdateJobRequest
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public EmploymentType EmploymentType { get; set; }
}
