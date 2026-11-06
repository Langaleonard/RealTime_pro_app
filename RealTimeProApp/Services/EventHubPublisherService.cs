using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using RealTimeProApp.Models;

namespace RealTimeProApp.Services;

public class EventHubPublisherService
{
    private readonly IConfiguration _configuration;

    public EventHubPublisherService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Azure:EventHubConnectionString"]);

    public async Task PublishOrderAsync(OrderEvent order)
    {
        string connectionString = _configuration["Azure:EventHubConnectionString"]
            ?? throw new InvalidOperationException("Event Hub connection string is not configured.");

        string eventHubName = _configuration["Azure:EventHubName"] ?? "order-stream";

        await using var producer = new EventHubProducerClient(connectionString, eventHubName);
        using EventDataBatch batch = await producer.CreateBatchAsync();

        string json = JsonSerializer.Serialize(order);

        if (!batch.TryAdd(new EventData(json)))
        {
            throw new InvalidOperationException("The order event is too large for the Event Hub batch.");
        }

        await producer.SendAsync(batch);
    }
}
