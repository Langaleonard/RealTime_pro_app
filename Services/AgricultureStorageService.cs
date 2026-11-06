using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using RealTimeProApp.Models;

namespace RealTimeProApp.Services;

public class AgricultureStorageService
{
    private static readonly List<TelemetryRecord> LocalTelemetry = new();
    private static readonly List<string> LocalCommands = new();
    private static readonly List<string> LocalLogs = new();

    private readonly TableClient? _telemetryTable;
    private readonly BlobContainerClient? _droneContainer;
    private readonly QueueClient? _irrigationQueue;
    private readonly ShareClient? _logShare;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AgricultureStorageService> _logger;

    public bool UsingAzure { get; }

    public AgricultureStorageService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<AgricultureStorageService> logger)
    {
        _environment = environment;
        _logger = logger;

        var connectionString = configuration["AzureStorage:ConnectionString"];
        UsingAzure = !string.IsNullOrWhiteSpace(connectionString);

        if (UsingAzure)
        {
            _telemetryTable = new TableClient(connectionString, "Telemetry");
            _droneContainer = new BlobContainerClient(connectionString, "drone-images");
            _irrigationQueue = new QueueClient(connectionString, "irrigation-commands");
            _logShare = new ShareClient(connectionString, "farm-logs");
        }
    }

    public async Task<TelemetryRecord> CaptureAsync(CaptureViewModel input)
    {
        var record = new TelemetryRecord
        {
            PartitionKey = "Telemetry",
            RowKey = Guid.NewGuid().ToString(),
            SensorId = input.SensorId.Trim(),
            FieldName = input.FieldName.Trim(),
            SoilMoisture = input.SoilMoisture,
            Temperature = input.Temperature,
            DroneNote = input.DroneNote?.Trim() ?? string.Empty,
            Status = input.SoilMoisture < 30 ? "Irrigation Required" : "Normal",
            ProcessedAt = DateTimeOffset.UtcNow
        };

        if (input.DroneImage is { Length: > 0 })
        {
            record.ImageName = await UploadDroneImageAsync(input.DroneImage);
        }

        if (UsingAzure)
        {
            try
            {
                await _telemetryTable!.CreateIfNotExistsAsync();
                await _telemetryTable.AddEntityAsync(record);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Azure Table Storage unavailable. Using local fallback.");
                LocalTelemetry.Insert(0, record);
            }
        }
        else
        {
            LocalTelemetry.Insert(0, record);
        }

        var log = $"{DateTime.UtcNow:u} | {record.SensorId} | {record.FieldName} | Moisture {record.SoilMoisture}% | {record.Status}";
        await WriteLogAsync(log);

        if (record.SoilMoisture < 30)
        {
            var command = $"START_IRRIGATION | {record.FieldName} | {record.SensorId} | Moisture {record.SoilMoisture}% | {DateTime.UtcNow:u}";
            await SendIrrigationCommandAsync(command);
        }

        return record;
    }

    public async Task<List<TelemetryRecord>> GetTelemetryAsync()
    {
        if (UsingAzure)
        {
            try
            {
                await _telemetryTable!.CreateIfNotExistsAsync();
                var records = new List<TelemetryRecord>();
                await foreach (var item in _telemetryTable.QueryAsync<TelemetryRecord>())
                {
                    records.Add(item);
                }

                return records
                    .OrderByDescending(x => x.ProcessedAt)
                    .Take(50)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read Azure telemetry. Showing local fallback.");
            }
        }

        return LocalTelemetry
            .OrderByDescending(x => x.ProcessedAt)
            .Take(50)
            .ToList();
    }

    public async Task<(Stream Content, string ContentType)?> GetDroneImageAsync(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName)) return null;

        if (UsingAzure)
        {
            try
            {
                var blob = _droneContainer!.GetBlobClient(imageName);
                var response = await blob.DownloadStreamingAsync();
                return (response.Value.Content, response.Value.Details.ContentType ?? "application/octet-stream");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read image from Azure Blob Storage.");
            }
        }

        var localPath = Path.Combine(_environment.WebRootPath, "uploads", imageName);
        if (!File.Exists(localPath)) return null;

        var extension = Path.GetExtension(localPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };

        return (File.OpenRead(localPath), contentType);
    }

    public async Task<List<string>> GetIrrigationCommandsAsync()
    {
        if (UsingAzure)
        {
            try
            {
                await _irrigationQueue!.CreateIfNotExistsAsync();
                var messages = (await _irrigationQueue.PeekMessagesAsync(maxMessages: 20)).Value;
                return messages.Select(x => x.MessageText).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read Azure Queue Storage.");
            }
        }

        return LocalCommands.Take(20).ToList();
    }

    public async Task<List<string>> GetOperationalLogsAsync()
    {
        if (UsingAzure)
        {
            try
            {
                await _logShare!.CreateIfNotExistsAsync();
                var root = _logShare.GetRootDirectoryClient();
                var names = new List<string>();
                await foreach (var item in root.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory) names.Add(item.Name);
                }
                return names.OrderByDescending(x => x).Take(30).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read Azure Files.");
            }
        }

        return LocalLogs.Take(30).ToList();
    }

    private async Task<string> UploadDroneImageAsync(IFormFile image)
    {
        var extension = Path.GetExtension(image.FileName);
        var name = $"{Guid.NewGuid():N}{extension}";

        if (UsingAzure)
        {
            try
            {
                await _droneContainer!.CreateIfNotExistsAsync(PublicAccessType.None);
                var blob = _droneContainer.GetBlobClient(name);
                await using var stream = image.OpenReadStream();
                await blob.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = image.ContentType }
                });
                return name;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Blob upload failed. Saving image locally.");
            }
        }

        var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadFolder);
        var localPath = Path.Combine(uploadFolder, name);
        await using var output = File.Create(localPath);
        await image.CopyToAsync(output);
        return name;
    }

    private async Task SendIrrigationCommandAsync(string command)
    {
        if (UsingAzure)
        {
            try
            {
                await _irrigationQueue!.CreateIfNotExistsAsync();
                await _irrigationQueue.SendMessageAsync(command);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Queue write failed. Storing command locally.");
            }
        }

        LocalCommands.Insert(0, command);
    }

    private async Task WriteLogAsync(string text)
    {
        if (UsingAzure)
        {
            try
            {
                await _logShare!.CreateIfNotExistsAsync();
                var root = _logShare.GetRootDirectoryClient();
                var fileName = $"telemetry-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.txt";
                var file = root.GetFileClient(fileName);
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                await file.CreateAsync(bytes.Length);
                await using var stream = new MemoryStream(bytes);
                await file.UploadRangeAsync(new HttpRange(0, bytes.Length), stream);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Azure Files write failed. Storing log locally.");
            }
        }

        LocalLogs.Insert(0, text);
    }
}
