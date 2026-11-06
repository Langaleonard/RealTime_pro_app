using Azure;
using Azure.Data.Tables;

namespace RealTimeProApp.Models;

public class TelemetryRecord : ITableEntity
{
    public string PartitionKey { get; set; } = "Telemetry";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string SensorId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public string DroneNote { get; set; } = string.Empty;
    public string Status { get; set; } = "Normal";
    public string ImageName { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
}
