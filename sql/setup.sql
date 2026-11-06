CREATE TABLE ProcessedOrders (
    OrderId NVARCHAR(50) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    ProcessedAt DATETIME2 NOT NULL
);
GO

CREATE INDEX IX_ProcessedOrders_ProcessedAt
ON ProcessedOrders (ProcessedAt DESC);
GO

-- Quick test data if needed
INSERT INTO ProcessedOrders (OrderId, Amount, Status, ProcessedAt)
VALUES
('ORD-001', 1500.00, 'high-value', SYSUTCDATETIME()),
('ORD-002', 299.00, 'standard', SYSUTCDATETIME()),
('ORD-003', 2200.00, 'high-value', SYSUTCDATETIME());
GO

SELECT * FROM ProcessedOrders ORDER BY ProcessedAt DESC;
GO

SELECT Status, SUM(Amount) AS TotalRevenue, COUNT(*) AS OrderCount
FROM ProcessedOrders
GROUP BY Status;
GO
