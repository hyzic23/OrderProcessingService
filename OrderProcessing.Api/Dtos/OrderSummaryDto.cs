namespace OrderProcessing.Api.Dtos;

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}