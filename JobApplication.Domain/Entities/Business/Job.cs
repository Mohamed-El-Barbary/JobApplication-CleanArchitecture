using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities.Business;

public class Job : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime? ClosedAt { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public JobStatus Status { get; set; }
    public int RecruiterId { get; set; }
    public Recruiter Recruiter { get; set; } = default!;
    public ICollection<JobCandidateApplication> Applications { get; set; } = [];
}
