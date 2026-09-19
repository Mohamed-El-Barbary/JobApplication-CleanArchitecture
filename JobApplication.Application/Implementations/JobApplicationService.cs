using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Entities.Identity;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Implementations;

public class JobApplicationService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager) : IJobApplicationService
{
    public async Task<JobApplicationResponse> CreateAsync(
        int jobId,
        CreateJobApplicationRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateApplicationRequest(request?.YearsOfExperience);

        var candidate = await GetOrCreateCandidateAsync(userId, cancellationToken);

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job with ID '{jobId}' was not found.");
        }

        if (job.Status != JobStatus.Open)
        {
            throw new InvalidOperationException("Cannot apply to a job that is not open.");
        }

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var existingApplication = applications.FirstOrDefault(a => a.CandidateId == candidate.Id && a.JobId == jobId);
        if (existingApplication != null)
        {
            throw new InvalidOperationException("Candidate has already applied to this job.");
        }

        var application = new JobCandidateApplication
        {
            CandidateId = candidate.Id,
            JobId = jobId,
            CoverLetter = request?.CoverLetter?.Trim(),
            ResumeUrl = request?.ResumeUrl?.Trim(),
            YearsOfExperience = request?.YearsOfExperience,
            Status = ApplicationStatus.Applied
        };

        await unitOfWork.Repository<JobCandidateApplication>().AddAsync(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(application);
    }

    public async Task<JobApplicationResponse> GetByIdAsync(
        int applicationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var application = applications.FirstOrDefault(a => a.Id == applicationId);
        if (application == null)
        {
            throw new KeyNotFoundException($"Job application with ID '{applicationId}' was not found.");
        }

        var candidate = await FindCandidateByUserIdAsync(userId, cancellationToken);
        var recruiter = await FindRecruiterByUserIdAsync(userId, cancellationToken);

        var jobs = await unitOfWork.Repository<Job>().GetAllAsync(cancellationToken);
        var job = jobs.FirstOrDefault(j => j.Id == application.JobId);

        bool isCandidateOwner = candidate != null && application.CandidateId == candidate.Id;
        bool isRecruiterOwner = recruiter != null && job != null && job.RecruiterId == recruiter.Id;

        if (!isCandidateOwner && !isRecruiterOwner)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this application.");
        }

        return MapToResponse(application);
    }

    public async Task<IReadOnlyCollection<JobApplicationResponse>> GetMyApplicationsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {

        var candidate = await GetOrCreateCandidateAsync(userId, cancellationToken);

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        return applications
            .Where(a => a.CandidateId == candidate.Id)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<IReadOnlyCollection<JobApplicationResponse>> GetJobApplicationsAsync(
        int jobId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var recruiter = await GetOrCreateRecruiterAsync(userId, cancellationToken);

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job with ID '{jobId}' was not found.");
        }

        if (job.RecruiterId != recruiter.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to view applications for this job.");
        }

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        return applications
            .Where(a => a.JobId == jobId)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<JobApplicationResponse> UpdateAsync(
        int applicationId,
        UpdateJobApplicationRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateApplicationRequest(request?.YearsOfExperience);

        var candidate = await GetOrCreateCandidateAsync(userId, cancellationToken);

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var application = applications.FirstOrDefault(a => a.Id == applicationId);
        if (application == null)
        {
            throw new KeyNotFoundException($"Job application with ID '{applicationId}' was not found.");
        }

        if (application.CandidateId != candidate.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this application.");
        }

        if (application.Status != ApplicationStatus.Applied)
        {
            throw new InvalidOperationException("Application can only be edited while in Applied status.");
        }

        application.CoverLetter = request?.CoverLetter?.Trim();
        application.ResumeUrl = request?.ResumeUrl?.Trim();
        application.YearsOfExperience = request?.YearsOfExperience;

        unitOfWork.Repository<JobCandidateApplication>().Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(application);
    }

    public async Task CancelAsync(
        int applicationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await GetOrCreateCandidateAsync(userId, cancellationToken);

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var application = applications.FirstOrDefault(a => a.Id == applicationId);
        if (application == null)
        {
            throw new KeyNotFoundException($"Job application with ID '{applicationId}' was not found.");
        }

        if (application.CandidateId != candidate.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to cancel this application.");
        }

        if (application.Status == ApplicationStatus.Accepted ||
            application.Status == ApplicationStatus.Rejected ||
            application.Status == ApplicationStatus.Cancelled)
        {
            throw new InvalidOperationException("Application cannot be cancelled in its current status.");
        }

        application.Status = ApplicationStatus.Cancelled;

        unitOfWork.Repository<JobCandidateApplication>().Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<JobApplicationResponse> UpdateStatusAsync(
        int applicationId,
        UpdateApplicationStatusRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request body cannot be null.");
        }

        if (request.Status == ApplicationStatus.Cancelled)
        {
            throw new InvalidOperationException("Recruiters cannot set application status to Cancelled.");
        }

        var recruiter = await GetOrCreateRecruiterAsync(userId, cancellationToken);

        var applications = await unitOfWork.Repository<JobCandidateApplication>().GetAllAsync(cancellationToken);
        var application = applications.FirstOrDefault(a => a.Id == applicationId);
        if (application == null)
        {
            throw new KeyNotFoundException($"Job application with ID '{applicationId}' was not found.");
        }

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(application.JobId, cancellationToken);
        if (job == null || job.RecruiterId != recruiter.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to update status for this job application.");
        }

        ValidateStatusTransition(application.Status, request.Status);

        application.Status = request.Status;

        unitOfWork.Repository<JobCandidateApplication>().Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(application);
    }

    private static void ValidateStatusTransition(ApplicationStatus current, ApplicationStatus target)
    {
        if (current == ApplicationStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot update status of a cancelled application.");
        }

        if (current == ApplicationStatus.Accepted && (target == ApplicationStatus.Interview || target == ApplicationStatus.Applied))
        {
            throw new InvalidOperationException($"Invalid status transition from {current} to {target}.");
        }

        if (current == ApplicationStatus.Rejected && (target == ApplicationStatus.Applied || target == ApplicationStatus.Interview))
        {
            throw new InvalidOperationException($"Invalid status transition from {current} to {target}.");
        }
    }

    private static void ValidateApplicationRequest(int? yearsOfExperience)
    {
        if (yearsOfExperience.HasValue && yearsOfExperience.Value < 0)
        {
            throw new ArgumentException("YearsOfExperience cannot be negative.", nameof(yearsOfExperience));
        }
    }

    private async Task<Candidate?> FindCandidateByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        Guid.TryParse(userId, out var userGuid);
        var candidates = await unitOfWork.Repository<Candidate>().GetAllAsync(cancellationToken);
        var candidate = candidates.FirstOrDefault(c => c.UserId == userGuid);

        if (candidate == null)
        {
            candidate = candidates.FirstOrDefault(c => c.UserId.ToString() == userId.ToString());
        }

        return candidate;
    }

    private async Task<Recruiter?> FindRecruiterByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        Guid.TryParse(userId, out var userGuid);

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == userGuid);

        if (recruiter == null)
        {
            recruiter = recruiters.FirstOrDefault(r => r.UserId.ToString() == userId.ToString());
        }

        return recruiter;
    }

    private async Task<Candidate> GetOrCreateCandidateAsync(string userId, CancellationToken cancellationToken)
    {
        Guid.TryParse(userId, out var userGuid);

        var candidates = await unitOfWork.Repository<Candidate>().GetAllAsync(cancellationToken);
        var candidate = candidates.FirstOrDefault(c => c.UserId == userGuid);

        if (candidate == null)
        {
            candidate = candidates.FirstOrDefault(c => c.UserId.ToString() == userId.ToString());
        }

        if (candidate == null)
        {
            var appUser = await userManager.FindByIdAsync(userId.ToString());
            string email = appUser?.Email ?? $"candidate{userId}@jobapplication.com";

            candidate = candidates.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (candidate == null)
            {
                candidate = new Candidate
                {
                    FullName = appUser?.FullName ?? "Candidate",
                    Email = email,
                    CVUrl = "",
                    UserId = appUser != null ? appUser.Id : Guid.NewGuid()
                };

                await unitOfWork.Repository<Candidate>().AddAsync(candidate);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return candidate;
    }

    private async Task<Recruiter> GetOrCreateRecruiterAsync(string userId, CancellationToken cancellationToken)
    {
        Guid.TryParse(userId, out var userGuid);

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == userGuid);

        if (recruiter == null)
        {
            recruiter = recruiters.FirstOrDefault(r => r.UserId.ToString() == userId.ToString());
        }

        if (recruiter == null)
        {
            var appUser = await userManager.FindByIdAsync(userId.ToString());
            string email = appUser?.Email ?? $"recruiter{userId}@jobapplication.com";

            recruiter = recruiters.FirstOrDefault(r => r.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (recruiter == null)
            {
                recruiter = new Recruiter
                {
                    FullName = appUser?.FullName ?? "Recruiter",
                    Email = email,
                    UserId = appUser != null ? appUser.Id : Guid.NewGuid()
                };

                await unitOfWork.Repository<Recruiter>().AddAsync(recruiter);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return recruiter;
    }

    private static JobApplicationResponse MapToResponse(JobCandidateApplication application)
    {
        return new JobApplicationResponse
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            JobId = application.JobId,
            ResumeUrl = application.ResumeUrl,
            YearsOfExperience = application.YearsOfExperience,
            Status = application.Status,
            CreatedAt = application.CreatedAt
        };
    }
}
