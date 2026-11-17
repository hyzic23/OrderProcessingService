using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Api.Services;

namespace OrderProcessing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IReportService _reportService;

    public DashboardController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _reportService.GetDashboardSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 5)
    {
        var products = await _reportService.GetTopSellingProductsAsync(count);
        return Ok(products);
    }
}