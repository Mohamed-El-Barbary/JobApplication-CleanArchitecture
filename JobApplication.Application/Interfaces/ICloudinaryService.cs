using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces;

public interface ICloudinaryService
{
    Task<(string PublicId, string Url, string FileName)> UploadCvAsync(
        IFormFile file,
        string candidateId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default);
}
