using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define relationships
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Data (Helps with realistic testing)
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Price = 1200.00m, StockQuantity = 50 },
            new Product { Id = 2, Name = "Mouse", Price = 25.50m, StockQuantity = 200 }
        );
        
        // Seed a dummy user for testing
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Email = "test@example.com", FullName = "Test User" }
        );
    }
}