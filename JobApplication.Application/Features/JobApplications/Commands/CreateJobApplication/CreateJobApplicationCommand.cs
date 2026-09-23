using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Commands.CreateJobApplication;

public sealed record CreateJobApplicationCommand(
    string? CoverLetter,
    string? ResumeUrl,
    int? YearsOfExperience,
    int JobId,
    Guid UserId) : IRequest<Result<JobApplicationResponse>>;
