using Microsoft.Data.SqlClient;
using RealTimeProApp.Models;

namespace RealTimeProApp.Services;

public class ProcessedOrderService
{
    private readonly IConfiguration _configuration;

    public ProcessedOrderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Azure:SqlConnectionString"]);

    public async Task<List<ProcessedOrder>> GetRecentAsync()
    {
        var results = new List<ProcessedOrder>();
        string? connectionString = _configuration["Azure:SqlConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return results;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT TOP 50 OrderId, Amount, Status, ProcessedAt
            FROM ProcessedOrders
            ORDER BY ProcessedAt DESC;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new ProcessedOrder
            {
                OrderId = reader.GetString(0),
                Amount = reader.GetDecimal(1),
                Status = reader.GetString(2),
                ProcessedAt = reader.GetDateTime(3)
            });
        }

        return results;
    }
}
