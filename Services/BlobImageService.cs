using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace intellectualconversationAPI.Services
{

    public class BlobImageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobImageService(
            BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadImageAsync(
            IFormFile image,
            string containerName)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    containerName);

            // Generate a unique key for the image.
            string extension =
                Path.GetExtension(image.FileName).ToLowerInvariant();

            string imageKey = $"{Guid.NewGuid()}{extension}";

            var blobClient =
                containerClient.GetBlobClient(imageKey);

            await using var stream = image.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                overwrite: false);

            return imageKey;
        }

        public async Task<string?> GenerateReadSasUrlAsync(
            string? imageKey,
            string containerName)
        {
            if (string.IsNullOrWhiteSpace(imageKey))
                return null;

            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    containerName);

            var blobClient =
                containerClient.GetBlobClient(imageKey);

            // Skip missing images.
            if (!(await blobClient.ExistsAsync()).Value)
                return null;

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = imageKey,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            sasBuilder.SetPermissions(
                BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        public async Task PopulateImageUrlsAsync<T>(
            IEnumerable<T> records,
            string containerName,
            Func<T, string?> getImageKey,
            Action<T, string?> setImageUrl)
        {
            foreach (var record in records)
            {
                var imageKey = getImageKey(record);

                var imageUrl = await GenerateReadSasUrlAsync(
                    imageKey,
                    containerName);

                setImageUrl(record, imageUrl);
            }
        }

        public async Task DeleteImageAsync(
            string imageKey,
            string containerName)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    containerName);

            var blobClient =
                containerClient.GetBlobClient(imageKey);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
