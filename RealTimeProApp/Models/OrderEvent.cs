using System.ComponentModel.DataAnnotations;

namespace RealTimeProApp.Models;

public class OrderEvent
{
    [Required]
    [Display(Name = "Order ID")]
    public string OrderId { get; set; } = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";

    [Required]
    [Range(0.01, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "User ID")]
    public string UserId { get; set; } = "U001";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
