using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Data;
using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Enums;

namespace OrderProcessing.Api.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context) => _context = context;

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var totalRevenue = await _context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);

        var totalOrders = await _context.Orders.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        return new DashboardSummaryDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalUsers = totalUsers
        };
    }

    public async Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(int topCount)
    {
        return await _context.OrderItems
            .Include(i => i.Order)
            .Where(i => i.Order.Status != OrderStatus.Cancelled)
            .GroupBy(i => i.ProductName)
            .Select(g => new TopSellingProductDto
            {
                ProductName = g.Key,
                TotalSold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(dto => dto.TotalSold)
            .Take(topCount)
            .ToListAsync();
    }
}