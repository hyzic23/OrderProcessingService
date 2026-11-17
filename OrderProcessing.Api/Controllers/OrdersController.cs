using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Services;

namespace OrderProcessing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        try
        {
            var result = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetCustomerOrders), new { email = result.CustomerEmail }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("customer/{email}")]
    public async Task<IActionResult> GetCustomerOrders(string email, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var orders = await _orderService.GetCustomerOrdersAsync(email, page, pageSize);
        return Ok(orders);
    }

    // In a real app, this would require an Admin role token. We simulate passing the admin email.
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromQuery] string adminEmail)
    {
        if (string.IsNullOrEmpty(adminEmail)) return BadRequest("Admin email is required to cancel.");
        
        var success = await _orderService.CancelOrderAsync(id, adminEmail);
        if (!success) return BadRequest("Order cannot be cancelled.");
        return Ok("Order cancelled successfully.");
    }
}