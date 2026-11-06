using System.Net;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetail_Functions;

public class UploadLogFile
{
    private readonly ILogger<UploadLogFile> _logger;

    public UploadLogFile(ILogger<UploadLogFile> logger)
    {
        _logger = logger;
    }

    [Function("UploadLogFile")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "UploadLogFile/{fileName}")]
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

            var shareClient =
                new ShareClient(
                    connectionString,
                    "log-files");

            await shareClient.CreateIfNotExistsAsync();

            var rootDirectory =
                shareClient.GetRootDirectoryClient();

            var fileClient =
                rootDirectory.GetFileClient(fileName);

            using var memoryStream =
                new MemoryStream();

            await req.Body.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            await fileClient.CreateAsync(
                memoryStream.Length);

            await fileClient.UploadRangeAsync(
                new Azure.HttpRange(
                    0,
                    memoryStream.Length),
                memoryStream);

            _logger.LogInformation(
                "File uploaded to Azure Files: {FileName}",
                fileName);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(
                $"File uploaded successfully: {fileName}");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error uploading log file.");

            var response =
                req.CreateResponse(HttpStatusCode.InternalServerError);

            await response.WriteStringAsync(
                $"Error: {ex.Message}");

            return response;
        }
    }
}