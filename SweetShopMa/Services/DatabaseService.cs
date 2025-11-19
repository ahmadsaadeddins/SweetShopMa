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
            await _database.CreateTableAsync<AttendanceRecord>();
            await _database.CreateTableAsync<RestockRecord>();

            await EnsureUserTableColumnsAsync();
            await EnsureAttendanceTableColumnsAsync();
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
        try
        {
            return await _database.Table<Product>().CountAsync();
        }
        catch (SQLiteException ex) when (ex.Message.Contains("no such table"))
        {
            // Table doesn't exist yet, create it and return 0
            await _database.CreateTableAsync<Product>();
            return 0;
        }
    }

    public async Task SeedProductsAsync()
    {
        await InitializeAsync();
        
        // Ensure Product table exists
        await _database.CreateTableAsync<Product>();
        
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

    public async Task<List<OrderItem>> GetAllOrderItemsAsync()
    {
        await InitializeAsync();
        return await _database.Table<OrderItem>().ToListAsync();
    }

    // Attendance
    public async Task<int> SaveAttendanceRecordAsync(AttendanceRecord record)
    {
        await InitializeAsync();
        if (record.Id != 0)
            return await _database.UpdateAsync(record);
        return await _database.InsertAsync(record);
    }

    public async Task<List<AttendanceRecord>> GetAttendanceRecordsAsync(DateTime? start = null, DateTime? end = null)
    {
        await InitializeAsync();
        var query = _database.Table<AttendanceRecord>();

        if (start.HasValue)
            query = query.Where(r => r.Date >= start.Value);
        if (end.HasValue)
            query = query.Where(r => r.Date <= end.Value);

        return await query
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<AttendanceRecord> GetAttendanceRecordAsync(int userId, DateTime date)
    {
        await InitializeAsync();
        var normalizedDate = date.Date;
        return await _database.Table<AttendanceRecord>()
            .Where(r => r.UserId == userId && r.Date == normalizedDate)
            .FirstOrDefaultAsync();
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

    public async Task<List<User>> GetUsersAsync()
    {
        await InitializeAsync();
        return await _database.Table<User>()
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        await InitializeAsync();
        return await _database.Table<User>()
            .Where(u => u.Username == username)
            .CountAsync() > 0;
    }

    public async Task<bool> ProductBarcodeExistsAsync(string barcode)
    {
        await InitializeAsync();
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        return await _database.Table<Product>()
            .Where(p => p.Barcode == barcode)
            .CountAsync() > 0;
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        await InitializeAsync();
        return await _database.UpdateAsync(user);
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

        // Create default developer user
        var developer = new User
        {
            Username = "ama",
            Password = PasswordHelper.HashPassword("AsrAma12@#"),
            Role = "Developer",
            Name = "ahmad",
            IsEnabled = true,
            MonthlySalary = 0m
        };
        await _database.InsertAsync(developer);
    }
    
    public async Task<bool> HasAnyUsersAsync()
    {
        await InitializeAsync();
        return await GetUserCountAsync() > 0;
    }

    private async Task EnsureUserTableColumnsAsync()
    {
        var columns = await _database.QueryAsync<TableInfo>("PRAGMA table_info(User);");

        if (!columns.Any(c => string.Equals(c.name, "IsEnabled", StringComparison.OrdinalIgnoreCase)))
        {
            await _database.ExecuteAsync("ALTER TABLE User ADD COLUMN IsEnabled INTEGER NOT NULL DEFAULT 1;");
        }

        if (!columns.Any(c => string.Equals(c.name, "MonthlySalary", StringComparison.OrdinalIgnoreCase)))
        {
            await _database.ExecuteAsync("ALTER TABLE User ADD COLUMN MonthlySalary REAL NOT NULL DEFAULT 0;");
        }
    }

    private async Task EnsureAttendanceTableColumnsAsync()
    {
        var columns = await _database.QueryAsync<TableInfo>("PRAGMA table_info(AttendanceRecord);");

        async Task AddColumnAsync(string name, string sqlType, string defaultValue)
        {
            await _database.ExecuteAsync($"ALTER TABLE AttendanceRecord ADD COLUMN {name} {sqlType} {defaultValue};");
        }

        if (!columns.Any(c => string.Equals(c.name, "CheckInTime", StringComparison.OrdinalIgnoreCase)))
        {
            await AddColumnAsync("CheckInTime", "TEXT", "DEFAULT NULL");
        }

        if (!columns.Any(c => string.Equals(c.name, "CheckOutTime", StringComparison.OrdinalIgnoreCase)))
        {
            await AddColumnAsync("CheckOutTime", "TEXT", "DEFAULT NULL");
        }

        if (!columns.Any(c => string.Equals(c.name, "RegularHours", StringComparison.OrdinalIgnoreCase)))
        {
            await AddColumnAsync("RegularHours", "REAL", "DEFAULT 0");
        }

        if (!columns.Any(c => string.Equals(c.name, "OvertimeHours", StringComparison.OrdinalIgnoreCase)))
        {
            await AddColumnAsync("OvertimeHours", "REAL", "DEFAULT 0");
        }

        if (!columns.Any(c => string.Equals(c.name, "DailyPay", StringComparison.OrdinalIgnoreCase)))
        {
            await AddColumnAsync("DailyPay", "REAL", "DEFAULT 0");
        }

        if (!columns.Any(c => string.Equals(c.name, "IsPresent", StringComparison.OrdinalIgnoreCase)))
        {
            await AddColumnAsync("IsPresent", "INTEGER", "NOT NULL DEFAULT 1");
        }
    }

    // Restock Record methods
    public async Task<int> CreateRestockRecordAsync(RestockRecord record)
    {
        await InitializeAsync();
        return await _database.InsertAsync(record);
    }

    public async Task<List<RestockRecord>> GetRestockRecordsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitializeAsync();
        var query = _database.Table<RestockRecord>();
        
        if (startDate.HasValue)
        {
            query = query.Where(r => r.RestockDate >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(r => r.RestockDate <= endDate.Value);
        }
        
        return await query.OrderByDescending(r => r.RestockDate).ToListAsync();
    }

    public async Task<List<RestockRecord>> GetRestockRecordsByProductAsync(int productId)
    {
        await InitializeAsync();
        return await _database.Table<RestockRecord>()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.RestockDate)
            .ToListAsync();
    }

    public async Task<List<RestockRecord>> GetRestockRecordsByUserAsync(int userId)
    {
        await InitializeAsync();
        return await _database.Table<RestockRecord>()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RestockDate)
            .ToListAsync();
    }

    private class TableInfo
    {
        // ReSharper disable once InconsistentNaming - matches PRAGMA output
        public string name { get; set; }
    }
}
