namespace OrderProcessing.Api.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty; // e.g., "Order Created"
    public string PerformedBy { get; set; } = string.Empty; // User Email or CustomerId
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}