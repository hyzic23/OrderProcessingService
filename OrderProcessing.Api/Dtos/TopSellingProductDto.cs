namespace OrderProcessing.Api.Dtos;

public class TopSellingProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}