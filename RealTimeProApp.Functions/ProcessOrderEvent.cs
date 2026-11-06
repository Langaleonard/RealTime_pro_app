using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RealTimeProApp.Functions;

public class ProcessOrderEvent
{
    private readonly ILogger<ProcessOrderEvent> _logger;

    public ProcessOrderEvent(ILogger<ProcessOrderEvent> logger)
    {
        _logger = logger;
    }

    [Function("ProcessOrderEvent")]
    public async Task Run(
        [EventHubTrigger("%EventHubName%", Connection = "EventHubConnectionString")] string[] events)
    {
        foreach (string eventBody in events)
        {
            try
            {
                OrderPayload? order = JsonSerializer.Deserialize<OrderPayload>(eventBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (order == null || string.IsNullOrWhiteSpace(order.OrderId))
                {
                    _logger.LogWarning("Skipped an invalid event.");
                    continue;
                }

                string status = order.Amount > 1000 ? "high-value" : "standard";
                await SqlOrderRepository.SaveAsync(order, status);
                _logger.LogInformation("Event Hub order {OrderId} stored as {Status}.", order.OrderId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Event Hub message: {EventBody}", eventBody);
                throw;
            }
        }
    }
}
