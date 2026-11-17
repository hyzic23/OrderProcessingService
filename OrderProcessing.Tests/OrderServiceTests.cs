using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Data;
using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Enums;
using OrderProcessing.Api.Models;
using OrderProcessing.Api.Services;

namespace OrderProcessing.Tests;

public class OrderServiceTests
{
    // Helper method to create a fresh database for every test
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // ==========================================
    // USER TESTS
    // ==========================================

    [Fact]
    public async Task CreateOrder_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new OrderService(context);
        var dto = new CreateOrderDto
        {
            CustomerEmail = "unknown@example.com",
            Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = 1 } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.CreateOrderAsync(dto));
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task CreateOrder_ShouldSucceed_WhenUserExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new OrderService(context);
        context.Products.Add(new Product { Id = 10, Name = "Test Item", Price = 10m, StockQuantity = 5 });
        context.Users.Add(new User { Id = 2, Email = "valid@example.com", FullName = "Valid User" });
        await context.SaveChangesAsync();

        var dto = new CreateOrderDto
        {
            CustomerEmail = "valid@example.com",
            Items = new List<CreateOrderItemDto> { new() { ProductId = 10, Quantity = 2 } }
        };

        // Act
        var result = await service.CreateOrderAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("valid@example.com", result.CustomerEmail);
        Assert.Equal(20m, result.TotalAmount); // 2 items * 10m
    }

    // ==========================================
    // AUDIT LOG TESTS
    // ==========================================

    [Fact]
    public async Task CreateOrder_ShouldCreateAuditLog()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new OrderService(context);
        
        context.Products.Add(new Product { Id = 20, Name = "Audit Test Item", Price = 5m, StockQuantity = 10 });
        context.Users.Add(new User { Id = 3, Email = "audit@example.com", FullName = "Audit User" });
        await context.SaveChangesAsync();

        var dto = new CreateOrderDto
        {
            CustomerEmail = "audit@example.com",
            Items = new List<CreateOrderItemDto> { new() { ProductId = 20, Quantity = 1 } }
        };

        // Act
        await service.CreateOrderAsync(dto);

        // Assert
        var auditLog = await context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.Contains("Order Created", auditLog.Action);
        Assert.Equal("audit@example.com", auditLog.PerformedBy);
    }

    [Fact]
    public async Task CancelOrder_ShouldCreateCancelAuditLog()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new OrderService(context);
        
        // Setup dummy order
        var order = new Order { Id = 99, CustomerId = "customer@example.com", Status = OrderStatus.Pending };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var success = await service.CancelOrderAsync(99, "admin@example.com");

        // Assert
        Assert.True(success);
        
        var auditLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action.Contains("Cancelled"));
        Assert.NotNull(auditLog);
        Assert.Equal("admin@example.com", auditLog.PerformedBy);
    }
    
    [Fact]
public async Task GetDashboardSummary_ShouldCalculateCorrectTotals()
{
    // Arrange
    using var context = GetInMemoryDbContext();
    var service = new ReportService(context);
    
    context.Users.Add(new User { Id = 10, Email = "u1@test.com", FullName = "U1" });
    context.Users.Add(new User { Id = 11, Email = "u2@test.com", FullName = "U2" });
    
    context.Orders.Add(new Order { Id = 100, CustomerId = "u1@test.com", TotalAmount = 150m, Status = OrderStatus.Processing });
    context.Orders.Add(new Order { Id = 101, CustomerId = "u2@test.com", TotalAmount = 50m, Status = OrderStatus.Cancelled }); // Should be ignored in revenue
    context.Orders.Add(new Order { Id = 102, CustomerId = "u1@test.com", TotalAmount = 200m, Status = OrderStatus.Shipped });
    
    await context.SaveChangesAsync();

    // Act
    var result = await service.GetDashboardSummaryAsync();

    // Assert
    Assert.Equal(350m, result.TotalRevenue); // 150 + 200 (Cancelled ignored)
    Assert.Equal(3, result.TotalOrders);
    Assert.Equal(2, result.TotalUsers);
}

[Fact]
public async Task GetTopSellingProducts_ShouldGroupAndOrderCorrectly()
{
    // Arrange
    using var context = GetInMemoryDbContext();
    var service = new ReportService(context);

    var order = new Order { Id = 200, Status = OrderStatus.Processing };
    context.Orders.Add(order);
    
    context.OrderItems.AddRange(
        new OrderItem { OrderId = 200, ProductName = "Laptop", Quantity = 2, UnitPrice = 1000m },
        new OrderItem { OrderId = 200, ProductName = "Mouse", Quantity = 5, UnitPrice = 20m },
        new OrderItem { OrderId = 200, ProductName = "Laptop", Quantity = 1, UnitPrice = 1000m }
    );
    
    await context.SaveChangesAsync();

    // Act
    var result = (await service.GetTopSellingProductsAsync(2)).ToList();

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal("Laptop", result[0].ProductName); // 3 sold total
    Assert.Equal(3, result[0].TotalSold);
    Assert.Equal(3000m, result[0].TotalRevenue);
    
    Assert.Equal("Mouse", result[1].ProductName); // 5 sold total
    Assert.Equal(5, result[1].TotalSold);
}
}