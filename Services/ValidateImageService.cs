
using Azure;
using intellectualconversationAPI.Services;

namespace intellectualconversationAPI.Services
{
    public class ValidateImageService : IValidateImageService
    {
        private readonly IProfileImageSafetyService? _profileImageSafetyService;
        private readonly string[] allowedImageTypes =
        {
            "image/jpeg",
            "image/jfif",
            "image/png",
            "image/webp"
        };

        public ValidateImageService(IProfileImageSafetyService profileImageSafetyService)
        {
            _profileImageSafetyService = profileImageSafetyService;
        }

        public async Task<string?> ValidateImagesAsync(List<IFormFile>? images, int maxPostImages = 1, long maxPostFileMaxLength = 1024 * 1024)
        {
            if (images == null || images.Count == 0)
            {
                return null;
            }

            if (images.Count > maxPostImages)
            {
                return $"A maximum of {maxPostImages} images is allowed.";
            }

            foreach (var image in images)
            {
                if (image.Length == 0)
                {
                    return "An uploaded image is empty.";
                }

                if (image.Length > maxPostFileMaxLength )
                {
                    return $"Each image must be {maxPostFileMaxLength / (1024 * 1024)} MB or smaller.";
                }

                if (!allowedImageTypes.Contains(
                    image.ContentType.ToLowerInvariant()))
                {
                    return "Unsupported image type.";
                }

                // Scan the profile image using Azure AI Content Safety
                try
                {
                    int sexualSeverity =
                        await _profileImageSafetyService.ScanProfileImageAsync(image);

                    // Reject images with sexual-content severity 4 or 6
                    if (sexualSeverity >= 4)
                    {
                        return "This profile picture contains prohibited sexual content. Please choose another image.";
                    }
                }
                catch (RequestFailedException)
                {
                    // Do not allow an image to bypass safety scanning
                    return "Image safety scanning is temporarily unavailable. Please try again later.";
                }
                catch (InvalidOperationException)
                {
                    return "Image safety scanning is not configured or could not return a result.";
                }

            }

            return null;
        }
    }
}
