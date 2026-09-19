using JobApplication.Application.DTOs.JobApplications;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces;

public interface IJobApplicationService
{
    Task<JobApplicationResponse> CreateAsync(
        int jobId,
        CreateJobApplicationRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<JobApplicationResponse> GetByIdAsync(
        int applicationId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<JobApplicationResponse>> GetMyApplicationsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<JobApplicationResponse>> GetJobApplicationsAsync(
        int jobId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<JobApplicationResponse> UpdateAsync(
        int applicationId,
        UpdateJobApplicationRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        int applicationId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<JobApplicationResponse> UpdateStatusAsync(
        int applicationId,
        UpdateApplicationStatusRequest request,
        string userId,
        CancellationToken cancellationToken = default);
}
