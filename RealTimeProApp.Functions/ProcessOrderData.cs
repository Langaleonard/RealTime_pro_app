using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace RealTimeProApp.Functions;

public class ProcessOrderData
{
    private readonly ILogger<ProcessOrderData> _logger;

    public ProcessOrderData(ILogger<ProcessOrderData> logger)
    {
        _logger = logger;
    }

    [Function("ProcessOrderData")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        string body = await new StreamReader(req.Body).ReadToEndAsync();
        OrderPayload? order = JsonSerializer.Deserialize<OrderPayload>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (order == null || string.IsNullOrWhiteSpace(order.OrderId) || order.Amount <= 0)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Provide OrderId and an Amount greater than 0.");
            return bad;
        }

        string status = order.Amount > 1000 ? "high-value" : "standard";
        await SqlOrderRepository.SaveAsync(order, status);

        _logger.LogInformation("Processed {OrderId} as {Status}", order.OrderId, status);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            order.OrderId,
            order.Amount,
            Status = status,
            ProcessedAt = DateTime.UtcNow,
            Message = "Data processed and saved to Azure SQL successfully."
        });

        return response;
    }
}
