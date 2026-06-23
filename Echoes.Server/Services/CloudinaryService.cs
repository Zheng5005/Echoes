using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Echoes.Server.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName is not configured.");
        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey is not configured.");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret is not configured.");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(string base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            throw new ArgumentException("Image data is empty.", nameof(base64Image));
        }

        // Ensure payload is a proper data URI for Cloudinary
        var upload = new ImageUploadParams
        {
            File = new FileDescription(base64Image),
            Folder = "echoes"
        };

        var result = await _cloudinary.UploadAsync(upload);

        if (result.Error is not null)
        {
            _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        return result.SecureUrl?.ToString()
            ?? throw new InvalidOperationException("Cloudinary returned no secure URL.");
    }
}
