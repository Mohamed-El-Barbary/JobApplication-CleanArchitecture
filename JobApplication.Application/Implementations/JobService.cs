using JobApplication.Application.DTOs.Jobs;
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

public class JobService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager) : IJobService
{
    public async Task<JobResponse> CreateAsync(
        CreateJobRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateJobRequest(request?.Title, request?.Description);

        var recruiter = await GetOrCreateRecruiterAsync(userId, cancellationToken);

        var job = new Job
        {
            Title = request!.Title.Trim(),
            Description = request.Description.Trim(),
            EmploymentType = request.EmploymentType,
            Status = JobStatus.Open,
            IsActive = true,
            ClosedAt = null,
            RecruiterId = recruiter.Id
        };

        await unitOfWork.Repository<Job>().AddAsync(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(job);
    }

    public async Task<JobResponse> GetByIdAsync(
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await unitOfWork.Repository<Job>().GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job with ID '{jobId}' was not found.");
        }

        return MapToResponse(job);
    }

    public async Task<IReadOnlyCollection<JobResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var jobs = await unitOfWork.Repository<Job>().GetAllAsync(cancellationToken);
        return jobs
            .Where(j => j.Status == JobStatus.Open)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<IReadOnlyCollection<JobResponse>> GetMyJobsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var recruiter = await GetOrCreateRecruiterAsync(userId, cancellationToken);
        var jobs = await unitOfWork.Repository<Job>().GetAllAsync(cancellationToken);

        return jobs
            .Where(j => j.RecruiterId == recruiter.Id)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<JobResponse> UpdateAsync(
        int jobId,
        UpdateJobRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateJobRequest(request?.Title, request?.Description);

        var job = await unitOfWork.Repository<Job>().GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job with ID '{jobId}' was not found.");
        }

        var recruiter = await GetOrCreateRecruiterAsync(userId, cancellationToken);
        if (job.RecruiterId != recruiter.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this job.");
        }

        job.Title = request!.Title.Trim();
        job.Description = request.Description.Trim();
        job.EmploymentType = request.EmploymentType;

        unitOfWork.Repository<Job>().Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(job);
    }

    public async Task DeleteAsync(
        int jobId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var job = await unitOfWork.Repository<Job>().GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job with ID '{jobId}' was not found.");
        }

        var recruiter = await GetOrCreateRecruiterAsync(userId, cancellationToken);
        if (job.RecruiterId != recruiter.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this job.");
        }

        unitOfWork.Repository<Job>().Delete(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CloseAsync(
        int jobId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var job = await unitOfWork.Repository<Job>().GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job with ID '{jobId}' was not found.");
        }

        var recruiter = await GetOrCreateRecruiterAsync(userId, cancellationToken);
        if (job.RecruiterId != recruiter.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to close this job.");
        }

        job.Status = JobStatus.Closed;
        job.ClosedAt = DateTime.UtcNow;

        unitOfWork.Repository<Job>().Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Recruiter> GetOrCreateRecruiterAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        Guid.TryParse(userId, out var userGuid);

        var recruiters = await unitOfWork.Repository<Recruiter>().GetAllAsync(cancellationToken);
        var recruiter = recruiters.FirstOrDefault(r => r.UserId == userGuid);

        if (recruiter == null)
        {
            var appUser = await userManager.FindByIdAsync(userId);
            recruiter = recruiters.FirstOrDefault(r => r.Email.Equals(appUser?.Email, StringComparison.OrdinalIgnoreCase));
            
            if (recruiter == null)
            {
                recruiter = new Recruiter
                {
                    FullName = appUser?.FullName ?? "Recruiter",
                    Email = appUser?.Email ?? "recruiter@jobapplication.com",
                    UserId = userGuid
                };
                await unitOfWork.Repository<Recruiter>().AddAsync(recruiter);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else if (recruiter.UserId == Guid.Empty)
            {
                recruiter.UserId = userGuid;
                unitOfWork.Repository<Recruiter>().Update(recruiter);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return recruiter;
    }

    private static void ValidateJobRequest(string? title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }
    }

    private static JobResponse MapToResponse(Job job)
    {
        return new JobResponse
        {
            Id = job.Id,
            RecruiterId = job.RecruiterId,
            Title = job.Title,
            Description = job.Description,
            EmploymentType = job.EmploymentType,
            Status = job.Status,
            ClosedAt = job.ClosedAt,
            CreatedAt = job.CreatedAt
        };
    }
}
