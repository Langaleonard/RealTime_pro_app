using Microsoft.Data.SqlClient;

namespace RealTimeProApp.Functions;

public static class SqlOrderRepository
{
    public static async Task SaveAsync(OrderPayload order, string status)
    {
        string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("SqlConnectionString is not configured.");
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO ProcessedOrders (OrderId, Amount, Status, ProcessedAt)
            VALUES (@OrderId, @Amount, @Status, @ProcessedAt);
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderId", order.OrderId);
        command.Parameters.AddWithValue("@Amount", order.Amount);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@ProcessedAt", DateTime.UtcNow);
        await command.ExecuteNonQueryAsync();
    }
}
