using CleanAPIDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAPIDemo.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static void SeedDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();

        if (context.Products.Any())
            return;

        var products = new List<Product>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Laptop Pro 15",
                Description = "High-performance laptop with 15-inch display, 16GB RAM, 512GB SSD",
                Price = 1299.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with adjustable DPI",
                Price = 49.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = null
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard with Cherry MX switches",
                Price = 149.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "4K Monitor 27\"",
                Description = "Ultra HD 4K monitor with HDR support and 144Hz refresh rate",
                Price = 599.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = null
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "USB-C Hub",
                Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader",
                Price = 79.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = null
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Noise Cancelling Headphones",
                Description = "Premium wireless headphones with active noise cancellation",
                Price = 349.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Webcam HD 1080p",
                Description = "Full HD webcam with built-in microphone and auto-focus",
                Price = 89.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = null
            },
            new()
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "External SSD 1TB",
                Description = "Portable SSD with USB 3.2 Gen 2 and 1050MB/s read speed",
                Price = 129.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = null
            },
            new()
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Name = "Desk Lamp LED",
                Description = "Adjustable LED desk lamp with multiple brightness levels",
                Price = 39.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = null
            },
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Laptop Stand",
                Description = "Aluminum laptop stand with adjustable height and angle",
                Price = 59.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
