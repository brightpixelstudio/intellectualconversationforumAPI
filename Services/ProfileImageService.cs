using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace intellectualconversationAPI.Services
{
    public class ProfileImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _profileImagesContainer;

        public ProfileImageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;

            _profileImagesContainer =
                configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "AzureStorage:ContainerName is missing.");
        }

        public async Task PopulateProfileImageUrlsAsync<T>(
            IEnumerable<T> records,
            Func<T, string?> getProfileImageKey,
            Action<T, string?> setProfileImageUrl)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    _profileImagesContainer);

            foreach (var record in records)
            {
                setProfileImageUrl(record, null);

                var profileImageKey = getProfileImageKey(record);

                if (string.IsNullOrWhiteSpace(profileImageKey))
                    continue;

                var blobClient =
                    containerClient.GetBlobClient(profileImageKey);

                // Skip missing images
                if (!(await blobClient.ExistsAsync()).Value)
                    continue;

                // Generate a read-only SAS URL valid for 30 minutes
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _profileImagesContainer,
                    BlobName = profileImageKey,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                sasBuilder.SetPermissions(
                    BlobSasPermissions.Read);

                setProfileImageUrl(
                    record,
                    blobClient.GenerateSasUri(sasBuilder).ToString());
            }
        }
    }
}
