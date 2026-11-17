using OrderProcessing.Api.Dtos;

namespace OrderProcessing.Api.Services;

public interface IOrderService
{
    Task<OrderSummaryDto> CreateOrderAsync(CreateOrderDto dto);
    Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(string email, int page, int pageSize);
    Task<bool> CancelOrderAsync(int orderId, string adminEmail);
}