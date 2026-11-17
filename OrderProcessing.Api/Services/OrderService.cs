using System.Net;
using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Data;
using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Enums;
using OrderProcessing.Api.Middleware;
using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    
    public OrderService(AppDbContext context) => _context = context;

    public async Task<OrderSummaryDto> CreateOrderAsync(CreateOrderDto dto)
    {
        // 1. Verify User Exists
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.CustomerEmail);
        if (user == null) 
            throw new AppExceptionMiddleware("User not found.", HttpStatusCode.NotFound);

        // 2. Validate Stock
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        if (products.Count != productIds.Count)
            throw new AppExceptionMiddleware("One or more products do not exist.", HttpStatusCode.BadRequest);;

        var order = new Order { CustomerId = user.Email };

        foreach (var item in dto.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (!product.IsActive) 
                throw new AppExceptionMiddleware($"Product {product.Name} is inactive.", HttpStatusCode.BadRequest);
            
            if (product.StockQuantity < item.Quantity) 
                throw new AppExceptionMiddleware($"Insufficient stock for {product.Name}.", HttpStatusCode.BadRequest);

            product.StockQuantity -= item.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
            
            order.TotalAmount += (product.Price * item.Quantity);
        }

        _context.Orders.Add(order);

        // 3. Create Audit Log
        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Order Created for {dto.Items.Sum(i => i.Quantity)} items",
            PerformedBy = user.Email
        });

        await _context.SaveChangesAsync();

        return new OrderSummaryDto
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            ItemCount = order.Items.Count
        };
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(string email, int page, int pageSize)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == email)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                CustomerEmail = o.CustomerId,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                ItemCount = o.Items.Count
            })
            .ToListAsync();
    }

    public async Task<bool> CancelOrderAsync(int orderId, string adminEmail)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null || order.Status == OrderStatus.Shipped) return false;

        order.Status = OrderStatus.Cancelled;
        
        // Refund stock
        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null) product.StockQuantity += item.Quantity;
        }

        // Create Audit Log
        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Order {orderId} Cancelled",
            PerformedBy = adminEmail
        });

        await _context.SaveChangesAsync();
        return true;
    }
}