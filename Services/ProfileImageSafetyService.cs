using Azure;
using Azure.AI.ContentSafety;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace intellectualconversationAPI.Services
{
    public class ProfileImageSafetyService : IProfileImageSafetyService
    {
        private readonly IConfiguration _configuration;
        private readonly IProfileImageSafetyService _profileImageSafetyService;

        public ProfileImageSafetyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> ScanProfileImageAsync(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                throw new ArgumentException(
                    "A profile image is required.",
                    nameof(profileImage));
            }

            string? endpoint =
                _configuration["AzureAIContentSafety:Endpoint"];

            string? key =
                _configuration["AzureAIContentSafety:Key"];

            if (string.IsNullOrWhiteSpace(endpoint) ||
                string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "Azure AI Content Safety is not configured.");
            }

            // Read the image into memory
            await using var memoryStream = new MemoryStream();

            await profileImage.CopyToAsync(memoryStream);

            var imageData = new ContentSafetyImageData(
                BinaryData.FromBytes(memoryStream.ToArray()));

            // Create the Azure client
            var client = new ContentSafetyClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));

            // Analyze sexual content
            var options = new AnalyzeImageOptions(imageData)
            {
                Categories = { ImageCategory.Sexual },
                OutputType = AnalyzeImageOutputType.FourSeverityLevels
            };

            var response = await client.AnalyzeImageAsync(options);

            var sexualResult = response.Value.CategoriesAnalysis
                .FirstOrDefault(x =>
                    x.Category == ImageCategory.Sexual);

            if (sexualResult?.Severity == null)
            {
                throw new InvalidOperationException(
                    "Azure did not return a sexual-content severity result.");
            }

            return sexualResult.Severity ?? 0;
        }
    }
}
