using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetail.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _containerClient =
                new BlobContainerClient(
                    connectionString,
                    "product-images");
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            string extension =
                Path.GetExtension(imageFile.FileName);

            string blobName =
                $"{Guid.NewGuid()}{extension}";

            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            using Stream stream = imageFile.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = imageFile.ContentType
                    }
                });

            return blobName;
        }

        public async Task<(Stream Content, string ContentType)>
            DownloadImageAsync(string blobName)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            var response =
                await blobClient.DownloadStreamingAsync();

            string contentType =
                response.Value.Details.ContentType
                ?? "application/octet-stream";

            return (
                response.Value.Content,
                contentType
            );
        }
    }
}