using Microsoft.Maui.Storage;
using SQLite;
using SweetShopMa.Models;
using SweetShopMa.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SweetShopMa.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;
    private const string DbFileName = "sweetshop.db3";
    private static readonly string DbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);

    public static string DatabasePath => DbPath;
    public static string AppDataDirectory => FileSystem.AppDataDirectory;

    // Ensure only one thread initializes the DB at a time
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    public DatabaseService()
    {
    }

    private async Task InitializeAsync()
    {
        if (_database is not null)
            return;

        await _initSemaphore.WaitAsync();
        try
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(DbPath);

            // Use WAL to reduce locking contention for concurrent readers/writers
            try
            {
                await _database.ExecuteAsync("PRAGMA journal_mode=WAL;");
            }
            catch
            {
                // If PRAGMA fails on a platform, don't crash initialization — WAL is an optimization
            }

            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Product>();
            await _database.CreateTableAsync<CartItem>();
            await _database.CreateTableAsync<Order>();
            await _database.CreateTableAsync<OrderItem>();
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        await InitializeAsync();
        return await _database.Table<CartItem>().ToListAsync();
    }

    public async Task<int> SaveCartItemAsync(CartItem item)
    {
        await InitializeAsync();
        if (item.Id != 0)
            return await _database.UpdateAsync(item);
        return await _database.InsertAsync(item);
    }

    public async Task<int> DeleteCartItemAsync(CartItem item)
    {
        await InitializeAsync();
        return await _database.DeleteAsync(item);
    }

    public async Task<int> ClearCartAsync()
    {
        await InitializeAsync();
        return await _database.DeleteAllAsync<CartItem>();
    }

    // Product methods
    public async Task<List<Product>> GetProductsAsync()
    {
        await InitializeAsync();
        var products = await _database.Table<Product>().ToListAsync();
        
        // Get sales data for each product from OrderItems
        var orderItems = await _database.Table<OrderItem>().ToListAsync();
        var salesByProduct = orderItems
            .GroupBy(oi => oi.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(oi => oi.Quantity)); // Keep as decimal for accurate weight-based sorting
        
        // Sort products by total sales (most sold first), then by name for products with same sales
        return products.OrderByDescending(p => 
            salesByProduct.TryGetValue(p.Id, out decimal sold) ? sold : 0m
        ).ThenBy(p => p.Name).ToList();
    }

    public async Task<int> SaveProductAsync(Product product)
    {
        await InitializeAsync();
        if (product.Id != 0)
            return await _database.UpdateAsync(product);
        return await _database.InsertAsync(product);
    }

    public async Task<int> DeleteProductAsync(Product product)
    {
        await InitializeAsync();
        return await _database.DeleteAsync(product);
    }

    public async Task<int> GetProductCountAsync()
    {
        await InitializeAsync();
        return await _database.Table<Product>().CountAsync();
    }

    public async Task SeedProductsAsync()
    {
        await InitializeAsync();
        var count = await GetProductCountAsync();
        if (count > 0) return; // Already seeded

        var products = new[]
        {
            new Product { Name = "Chocolate Cake", Emoji = "🍰", Barcode = "501", Price = 4.99m, Stock = 50m, IsSoldByWeight = false },
            new Product { Name = "Gummy Bears", Emoji = "🫐", Barcode = "502", Price = 3.49m, Stock = 100m, IsSoldByWeight = false },
            new Product { Name = "Lollipop", Emoji = "🍭", Barcode = "503", Price = 1.99m, Stock = 200m, IsSoldByWeight = false },
            new Product { Name = "Donut", Emoji = "🍩", Barcode = "504", Price = 2.49m, Stock = 75m, IsSoldByWeight = false },
            new Product { Name = "Ice Cream", Emoji = "🍦", Barcode = "505", Price = 3.99m, Stock = 60m, IsSoldByWeight = false },
            new Product { Name = "Candy Corn", Emoji = "🌽", Barcode = "506", Price = 2.99m, Stock = 150m, IsSoldByWeight = false },
            new Product { Name = "Cupcake", Emoji = "🧁", Barcode = "507", Price = 3.99m, Stock = 80m, IsSoldByWeight = false },
            new Product { Name = "Chocolate Bar", Emoji = "🍫", Barcode = "508", Price = 2.49m, Stock = 120m, IsSoldByWeight = true }, // Sold by kilo
            new Product { Name = "Marshmallow", Emoji = "☁️", Barcode = "509", Price = 1.99m, Stock = 90m, IsSoldByWeight = false },
            new Product { Name = "Candy Apple", Emoji = "🍎", Barcode = "510", Price = 3.49m, Stock = 40m, IsSoldByWeight = false },
            new Product { Name = "Waffle", Emoji = "🧇", Barcode = "511", Price = 4.49m, Stock = 30m, IsSoldByWeight = false },
            new Product { Name = "Croissant", Emoji = "🥐", Barcode = "512", Price = 3.49m, Stock = 55m, IsSoldByWeight = false }
        };

        foreach (var product in products)
        {
            await _database.InsertAsync(product);
        }
    }

    // Order methods
    public async Task<int> CreateOrderAsync(Order order)
    {
        await InitializeAsync();
        var result = await _database.InsertAsync(order);
        // InsertAsync returns the number of rows affected, but with AutoIncrement,
        // the Id property is automatically set on the order object
        return order.Id;
    }

    public async Task<int> CreateOrderItemAsync(OrderItem orderItem)
    {
        await InitializeAsync();
        return await _database.InsertAsync(orderItem);
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        await InitializeAsync();
        return await _database.Table<Order>()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
    {
        await InitializeAsync();
        return await _database.Table<OrderItem>()
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<Order> GetOrderAsync(int orderId)
    {
        await InitializeAsync();
        return await _database.Table<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    // Inventory methods
    public async Task<bool> UpdateProductStockAsync(int productId, decimal quantityChange)
    {
        await InitializeAsync();
        var product = await _database.Table<Product>()
            .FirstOrDefaultAsync(p => p.Id == productId);
        
        if (product == null) return false;
        
        // product.Stock is decimal now — preserve fractional kilos
        product.Stock = product.Stock + quantityChange;
        if (product.Stock < 0m) product.Stock = 0m; // Prevent negative stock
        
        await _database.UpdateAsync(product);
        return true;
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        await InitializeAsync();
        return await _database.Table<Product>()
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<bool> CheckStockAvailabilityAsync(int productId, decimal requestedQuantity)
    {
        await InitializeAsync();
        var product = await GetProductAsync(productId);
        return product != null && product.Stock >= requestedQuantity;
    }

    // User methods
    public async Task<User> GetUserByUsernameAsync(string username)
    {
        await InitializeAsync();
        return await _database.Table<User>()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<int> CreateUserAsync(User user)
    {
        await InitializeAsync();
        return await _database.InsertAsync(user);
    }

    public async Task<int> GetUserCountAsync()
    {
        await InitializeAsync();
        return await _database.Table<User>().CountAsync();
    }

    public async Task SeedUsersAsync()
    {
        await InitializeAsync();
        var count = await GetUserCountAsync();
        if (count > 0) return; // Already seeded

        // Create default admin user
        var admin = new User
        {
            Username = "admin",
            Password = PasswordHelper.HashPassword("admin123"), // In production, this should be hashed
            Role = "Admin",
            Name = "Administrator"
        };
        await _database.InsertAsync(admin);

        // Create default customer user
        var customer = new User
        {
            Username = "customer",
            Password = PasswordHelper.HashPassword("customer123"), // In production, this should be hashed
            Role = "Customer",
            Name = "Customer"
        };
        await _database.InsertAsync(customer);
    }
}
