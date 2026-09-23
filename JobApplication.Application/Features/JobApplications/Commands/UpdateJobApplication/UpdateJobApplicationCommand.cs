using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplications.Commands.UpdateJobApplication;

public sealed record UpdateJobApplicationCommand(
    string? CoverLetter,
    string? ResumeUrl,
    int? YearsOfExperience, 
    int ApplicationId,
    Guid UserId) : IRequest<Result<JobApplicationResponse>>;
