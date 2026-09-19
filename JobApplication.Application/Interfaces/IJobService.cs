namespace JobApplication.Application.Interfaces;

using JobApplication.Application.DTOs.Jobs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IJobService
{
    Task<JobResponse> CreateAsync(
        CreateJobRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<JobResponse> GetByIdAsync(
        int jobId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<JobResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<JobResponse>> GetMyJobsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<JobResponse> UpdateAsync(
        int jobId,
        UpdateJobRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int jobId,
        string userId,
        CancellationToken cancellationToken = default);

    Task CloseAsync(
        int jobId,
        string userId,
        CancellationToken cancellationToken = default);
}
