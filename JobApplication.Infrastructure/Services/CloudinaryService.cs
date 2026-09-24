using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<(string PublicId, string Url, string FileName)> UploadCvAsync(
        IFormFile file,
        string candidateId,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("CV file is empty.", nameof(file));

        await using var stream = file.OpenReadStream();

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "job-application-system/cvs",
            PublicId = $"candidates/{candidateId}/cv"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");

        return (
            uploadResult.PublicId,
            uploadResult.SecureUrl.ToString(),
            file.FileName);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DelResParams
        {
            PublicIds = [publicId],
            ResourceType = ResourceType.Raw
        };
        await _cloudinary.DeleteResourcesAsync(deleteParams, cancellationToken);
    }
}
