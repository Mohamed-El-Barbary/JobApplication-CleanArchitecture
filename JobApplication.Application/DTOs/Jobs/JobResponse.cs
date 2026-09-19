namespace JobApplication.Application.DTOs.Jobs;

using JobApplication.Domain.Enums;
using System;

public class JobResponse
{
    public int Id { get; set; }

    public int RecruiterId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public EmploymentType EmploymentType { get; set; }

    public JobStatus Status { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
