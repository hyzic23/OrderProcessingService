using OrderProcessing.Api.Enums;

namespace OrderProcessing.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    
    // Navigation property
    public List<OrderItem> Items { get; set; } = new();
}