using System;

namespace JobApplication.Application.DTOs.Candidates;

public class CandidateProfileResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public string? Skills { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    
    // CV Info
    public string? CVUrl { get; set; }
    public string? CVFileName { get; set; }
    public DateTime? CVUploadedAt { get; set; }
}
