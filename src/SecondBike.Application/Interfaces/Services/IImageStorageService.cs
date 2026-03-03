namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Abstraction for cloud image storage (Cloudinary, S3, etc.).
/// Keeps infrastructure details out of application/domain layers.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Uploads an image and returns the public URL.
    /// </summary>
    /// <param name="imageStream">The image file stream.</param>
    /// <param name="fileName">Original file name (used for format detection).</param>
    /// <param name="folder">Cloudinary folder to organize uploads.</param>
    /// <returns>The public URL of the uploaded image.</returns>
    Task<string> UploadAsync(Stream imageStream, string fileName, string folder = "avatars");

    /// <summary>
    /// Deletes an image by its public URL or public ID.
    /// </summary>
    Task DeleteAsync(string imageUrl);
}
