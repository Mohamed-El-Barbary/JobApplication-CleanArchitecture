using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities.Business;

public class Candidate : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string CVUrl { get; set; } = default!;
    public Guid UserId { get; set; }

    public ICollection<JobCandidateApplication> JobApplications { get; set; } = [];
}
