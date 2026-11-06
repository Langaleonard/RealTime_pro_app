using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions
{
    public class StoreCustomer
    {
        private readonly ILogger<StoreCustomer> _logger;

        public StoreCustomer(ILogger<StoreCustomer> logger)
        {
            _logger = logger;
        }

        [Function("StoreCustomer")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData req)
        {
            _logger.LogInformation("StoreCustomer function started.");

            string requestBody =
                await new StreamReader(req.Body).ReadToEndAsync();

            CustomerRequest? customer =
                JsonSerializer.Deserialize<CustomerRequest>(
                    requestBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (customer == null ||
                string.IsNullOrWhiteSpace(customer.FirstName) ||
                string.IsNullOrWhiteSpace(customer.LastName) ||
                string.IsNullOrWhiteSpace(customer.Email))
            {
                var badResponse =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                await badResponse.WriteStringAsync(
                    "Please provide FirstName, LastName and Email.");

                return badResponse;
            }

            string connectionString =
                Environment.GetEnvironmentVariable(
                    "AzureStorageConnectionString")
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            TableClient tableClient =
                new TableClient(connectionString, "Customers");

            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(
                "Customer",
                Guid.NewGuid().ToString())
            {
                ["FirstName"] = customer.FirstName,
                ["LastName"] = customer.LastName,
                ["Email"] = customer.Email,
                ["PhoneNumber"] = customer.PhoneNumber ?? "",
                ["Address"] = customer.Address ?? ""
            };

            await tableClient.AddEntityAsync(entity);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(
                "Customer stored successfully in Azure Table Storage.");

            return response;
        }
    }

    public class CustomerRequest
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}