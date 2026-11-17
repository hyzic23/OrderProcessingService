using OrderProcessing.Api.Dtos;

namespace OrderProcessing.Api.Services;

public interface IReportService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(int topCount);
}