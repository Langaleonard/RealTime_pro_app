namespace RealTimeProApp.Functions;

public class OrderPayload
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
