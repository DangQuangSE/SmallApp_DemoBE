using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Cloudinary implementation of IImageStorageService.
/// </summary>
public class CloudinaryService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var settings = configuration.GetSection("Cloudinary");
        var account = new Account(
            settings["CloudName"],
            settings["ApiKey"],
            settings["ApiSecret"]);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadAsync(Stream imageStream, string fileName, string folder = "avatars")
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, imageStream),
            Folder = $"secondbike/{folder}",
            Transformation = folder == "avatars"
                ? new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                : new Transformation().Width(1200).Height(900).Crop("limit").Quality("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");

        _logger.LogInformation("Image uploaded to Cloudinary: {PublicId}", result.PublicId);
        return result.SecureUrl.ToString();
    }

    public async Task DeleteAsync(string imageUrl)
    {
        var publicId = ExtractPublicId(imageUrl);
        if (string.IsNullOrEmpty(publicId)) return;

        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        if (result.Result == "ok")
            _logger.LogInformation("Image deleted from Cloudinary: {PublicId}", publicId);
        else
            _logger.LogWarning("Failed to delete image from Cloudinary: {PublicId}, Result: {Result}", publicId, result.Result);
    }

    private static string? ExtractPublicId(string imageUrl)
    {
        // Cloudinary URL format: https://res.cloudinary.com/{cloud}/image/upload/v{version}/{folder}/{publicId}.{ext}
        try
        {
            var uri = new Uri(imageUrl);
            var path = uri.AbsolutePath; // /image/upload/v123/secondbike/avatars/abc123.jpg
            var uploadIndex = path.IndexOf("/upload/", StringComparison.Ordinal);
            if (uploadIndex < 0) return null;

            var afterUpload = path[(uploadIndex + 8)..]; // v123/secondbike/avatars/abc123.jpg
            // Skip version segment
            var slashIndex = afterUpload.IndexOf('/');
            if (slashIndex < 0) return null;

            var publicIdWithExt = afterUpload[(slashIndex + 1)..]; // secondbike/avatars/abc123.jpg
            var dotIndex = publicIdWithExt.LastIndexOf('.');
            return dotIndex > 0 ? publicIdWithExt[..dotIndex] : publicIdWithExt;
        }
        catch
        {
            return null;
        }
    }
}
