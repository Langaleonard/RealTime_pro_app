using System.Net;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetail_Functions;

public class ProcessOrder
{
    private readonly ILogger<ProcessOrder> _logger;

    public ProcessOrder(ILogger<ProcessOrder> logger)
    {
        _logger = logger;
    }

    [Function("ProcessOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "ProcessOrder")]
        HttpRequestData req)
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

            var queueClient =
                new QueueClient(
                    connectionString,
                    "order-processing");

            await queueClient.CreateIfNotExistsAsync();

            using var reader =
                new StreamReader(req.Body);

            var message =
                await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(message))
            {
                var badResponse =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                await badResponse.WriteStringAsync(
                    "Please provide an order message.");

                return badResponse;
            }

            await queueClient.SendMessageAsync(message);

            _logger.LogInformation(
                "Queue message added: {Message}",
                message);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(
                "Order message added to Azure Queue Storage.");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing order.");

            var response =
                req.CreateResponse(HttpStatusCode.InternalServerError);

            await response.WriteStringAsync(
                $"Error: {ex.Message}");

            return response;
        }
    }
}