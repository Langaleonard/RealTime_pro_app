using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetail_Functions;

public class UploadProductImage
{
    private readonly ILogger<UploadProductImage> _logger;

    public UploadProductImage(ILogger<UploadProductImage> logger)
    {
        _logger = logger;
    }

    [Function("UploadProductImage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "UploadProductImage/{fileName}")]
        HttpRequestData req,
        string fileName)
    {
        try
        {
            var connectionString =
                Environment.GetEnvironmentVariable("AzureStorageConnectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "Azure Storage connection string is missing.");

                return errorResponse;
            }

            var containerClient =
                new BlobContainerClient(
                    connectionString,
                    "product-images");

            await containerClient.CreateIfNotExistsAsync();

            var uniqueFileName =
                $"{Guid.NewGuid()}-{fileName}";

            var blobClient =
                containerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(
                req.Body,
                overwrite: true);

            _logger.LogInformation(
                "Uploaded blob: {FileName}",
                uniqueFileName);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(
                $"Image uploaded successfully: {uniqueFileName}");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error uploading product image.");

            var response =
                req.CreateResponse(HttpStatusCode.InternalServerError);

            await response.WriteStringAsync(
                $"Error: {ex.Message}");

            return response;
        }
    }
}