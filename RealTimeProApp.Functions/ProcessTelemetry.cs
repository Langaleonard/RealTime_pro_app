using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace RealTimeProApp.Functions;

public class ProcessTelemetry
{
    private readonly ILogger<ProcessTelemetry> _logger;

    public ProcessTelemetry(ILogger<ProcessTelemetry> logger) => _logger = logger;

    [Function("ProcessTelemetry")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ProcessTelemetry")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var reading = JsonSerializer.Deserialize<SensorReading>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (reading == null || string.IsNullOrWhiteSpace(reading.SensorId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Provide sensorId, fieldName, soilMoisture and temperature.");
            return bad;
        }

        var status = reading.SoilMoisture < 30 ? "Irrigation Required" : "Normal";

        if (status == "Irrigation Required")
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var queue = new QueueClient(connectionString, "irrigation-commands");
                await queue.CreateIfNotExistsAsync();
                await queue.SendMessageAsync($"START_IRRIGATION | {reading.FieldName} | {reading.SensorId} | Moisture {reading.SoilMoisture}%");
            }
        }

        _logger.LogInformation("Processed telemetry for {SensorId}: {Status}", reading.SensorId, status);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new
        {
            reading.SensorId,
            reading.FieldName,
            reading.SoilMoisture,
            reading.Temperature,
            Status = status,
            ProcessedAt = DateTime.UtcNow
        }));
        return response;
    }
}

public class SensorReading
{
    public string SensorId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
}
