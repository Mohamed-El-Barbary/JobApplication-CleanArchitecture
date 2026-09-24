using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities.Business;

public class Candidate : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string CVUrl { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public string? Skills { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    
    public string? CVPublicId { get; set; }
    public string? CVFileName { get; set; }
    public DateTime? CVUploadedAt { get; set; }

    public Guid UserId { get; set; }

    public ICollection<JobCandidateApplication> JobApplications { get; set; } = [];
}
