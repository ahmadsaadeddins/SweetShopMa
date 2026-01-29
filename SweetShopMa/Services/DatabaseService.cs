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

/// <summary>
/// Manages all database operations for the application.
/// 
/// WHAT IS DATABASESERVICE?
/// DatabaseService is the central service that handles all interactions with the SQLite database.
/// It provides methods for creating, reading, updating, and deleting (CRUD) data.
/// 
/// KEY RESPONSIBILITIES:
/// - Initialize database and create tables
/// - CRUD operations for all models (User, Product, Order, CartItem, etc.)
/// - Seed initial data (default Developer user, sample products)
/// - Thread-safe database initialization
/// 
/// DATABASE LOCATION:
/// Database file: "sweetshop.db3" stored in the app's data directory
/// - Windows: %AppData%\Local\Packages\[AppName]\LocalState\
/// - Android: /data/data/[AppName]/files/
/// - iOS: App's Documents directory
/// 
/// THREAD SAFETY:
/// Uses SemaphoreSlim to ensure only one thread initializes the database at a time.
/// This prevents race conditions when multiple threads try to access the database simultaneously.
/// 
/// WAL MODE:
/// Uses Write-Ahead Logging (WAL) mode for better concurrency. This allows multiple
/// readers and one writer to access the database simultaneously without blocking.
/// </summary>
public class DatabaseService
{
    // ============================================
    // DATABASE CONNECTION AND CONFIGURATION
    // ============================================
    
    /// <summary>
    /// SQLite database connection (lazy-initialized).
    /// null until InitializeAsync() is called.
    /// </summary>
    private SQLiteAsyncConnection _database;
    
    /// <summary>
    /// Database filename.
    /// </summary>
    private const string DbFileName = "sweetshop.db3";
    
    /// <summary>
    /// Full path to the database file.
    /// Uses FileSystem.AppDataDirectory which is platform-specific.
    /// </summary>
    private static readonly string DbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);

    /// <summary>
    /// Public property to get the database file path (for admin panel display).
    /// </summary>
    public static string DatabasePath => DbPath;
    
    /// <summary>
    /// Public property to get the app data directory (for admin panel display).
    /// </summary>
    public static string AppDataDirectory => FileSystem.AppDataDirectory;

    /// <summary>
    /// Semaphore to ensure thread-safe database initialization.
    /// Only allows one thread to initialize at a time (prevents race conditions).
    /// </summary>
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    /// <summary>
    /// Constructor (empty - database is initialized lazily on first use).
    /// </summary>
    public DatabaseService()
    {
    }

    /// <summary>
    /// Initializes the database connection and creates all tables if they don't exist.
    /// This method is called automatically before any database operation.
    /// 
    /// HOW IT WORKS:
    /// 1. Check if database is already initialized (return early if yes)
    /// 2. Acquire semaphore lock (wait if another thread is initializing)
    /// 3. Double-check initialization (another thread might have initialized while waiting)
    /// 4. Create database connection
    /// 5. Enable WAL mode (for better concurrency)
    /// 6. Create all tables
    /// 7. Ensure table columns exist (for database migrations)
    /// 8. Release semaphore lock
    /// 
    /// THREAD SAFETY:
    /// Uses double-checked locking pattern with SemaphoreSlim to prevent
    /// multiple threads from initializing the database simultaneously.
    /// </summary>
    private async Task InitializeAsync()
    {
        // Fast path: if already initialized, return immediately
        if (_database is not null)
            return;

        // Acquire lock (wait if another thread is initializing)
        await _initSemaphore.WaitAsync();
        try
        {
            // Double-check: another thread might have initialized while we were waiting
            if (_database is not null)
                return;

            // Create database connection
            _database = new SQLiteAsyncConnection(DbPath);

            // Enable WAL (Write-Ahead Logging) mode for better concurrency
            // WAL allows multiple readers and one writer simultaneously
            try
            {
                await _database.ExecuteAsync("PRAGMA journal_mode=WAL;");
            }
            catch
            {
                // If PRAGMA fails on a platform, don't crash initialization
                // WAL is an optimization, not required for functionality
            }

            // Create all database tables (if they don't exist)
            // These correspond to our Model classes
            await _database.CreateTableAsync<User>();              // User accounts
            await _database.CreateTableAsync<Product>();            // Products/inventory
            await _database.CreateTableAsync<CartItem>();           // Shopping cart items
            await _database.CreateTableAsync<Order>();              // Completed orders
            await _database.CreateTableAsync<OrderItem>();           // Items in orders
            await _database.CreateTableAsync<AttendanceRecord>();    // Employee attendance
            await _database.CreateTableAsync<RestockRecord>();      // Inventory restock history
            await _database.CreateTableAsync<EmployeeExpense>();
            await _database.CreateTableAsync<ShopSettings>();
            await _database.CreateTableAsync<ShopLocation>();
            await _database.CreateTableAsync<ProductStock>();
            await _database.CreateTableAsync<StockTransfer>();
            await _database.CreateTableAsync<UserLocation>();

            // One-time migration of stock to ProductStock table
            await MigrateProductStockAsync();

            // Ensure table columns exist (for database migrations)
            // These methods add new columns to existing tables if the app is updated
            await EnsureUserTableColumnsAsync();
            await EnsureAttendanceTableColumnsAsync();
            await EnsureProductTableColumnsAsync();
            try
            {
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_order_orderdate ON \"Order\"(OrderDate);");
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_orderitem_orderid ON \"OrderItem\"(OrderId);");
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_orderitem_productid ON \"OrderItem\"(ProductId);");
                await _database.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS idx_attendance_user_date ON \"AttendanceRecord\"(UserId, Date);");
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_attendance_user_date_range ON \"AttendanceRecord\"(UserId, Date);");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating database indexes: {ex}");
                throw;
            }
        }
        finally
        {
            // Always release the lock, even if an exception occurs
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

    /// <summary>
    /// Executes a database transaction safely.
    /// </summary>
    public async Task RunInTransactionAsync(Action<SQLiteConnection> action)
    {
        await InitializeAsync();
        await _database.RunInTransactionAsync(action);
    }

    // Product methods
    public async Task<List<Product>> GetProductsAsync()
    {
        await InitializeAsync();
        
        // Optimizing: Using a single query to get products with their total sales
        // This is much faster than running a separate query and combining in memory
        var query = @"
            SELECT p.*, IFNULL(s.TotalSold, 0) as TotalSold 
            FROM Product p
            LEFT JOIN (
                SELECT ProductId, SUM(Quantity) as TotalSold 
                FROM OrderItem 
                GROUP BY ProductId
            ) s ON p.Id = s.ProductId
            ORDER BY TotalSold DESC, p.Name ASC";
            
        return await _database.QueryAsync<Product>(query);
    }

    private class ProductSalesData
    {
        public int ProductId { get; set; }
        public decimal TotalSold { get; set; }
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
            new Product { Name = "Chocolate Cake", Emoji = "🍰", Barcode = "501", Price = 4.99m, Stock = 50m, IsSoldByWeight = false, Category = "Cakes" },
            new Product { Name = "Gummy Bears", Emoji = "🫐", Barcode = "502", Price = 3.49m, Stock = 100m, IsSoldByWeight = false, Category = "Candy" },
            new Product { Name = "Lollipop", Emoji = "🍭", Barcode = "503", Price = 1.99m, Stock = 200m, IsSoldByWeight = false, Category = "Candy" },
            new Product { Name = "Donut", Emoji = "🍩", Barcode = "504", Price = 2.49m, Stock = 75m, IsSoldByWeight = false, Category = "Pastries" },
            new Product { Name = "Ice Cream", Emoji = "🍦", Barcode = "505", Price = 3.99m, Stock = 60m, IsSoldByWeight = false, Category = "Frozen" },
            new Product { Name = "Candy Corn", Emoji = "🌽", Barcode = "506", Price = 2.99m, Stock = 150m, IsSoldByWeight = false, Category = "Candy" },
            new Product { Name = "Cupcake", Emoji = "🧁", Barcode = "507", Price = 3.99m, Stock = 80m, IsSoldByWeight = false, Category = "Cakes" },
            new Product { Name = "Chocolate Bar", Emoji = "🍫", Barcode = "508", Price = 2.49m, Stock = 120m, IsSoldByWeight = true, Category = "Candy" }, // Sold by kilo
            new Product { Name = "Marshmallow", Emoji = "☁️", Barcode = "509", Price = 1.99m, Stock = 90m, IsSoldByWeight = false, Category = "Candy" },
            new Product { Name = "Candy Apple", Emoji = "🍎", Barcode = "510", Price = 3.49m, Stock = 40m, IsSoldByWeight = false, Category = "Candy" },
            new Product { Name = "Waffle", Emoji = "🧇", Barcode = "511", Price = 4.49m, Stock = 30m, IsSoldByWeight = false, Category = "Pastries" },
            new Product { Name = "Croissant", Emoji = "🥐", Barcode = "512", Price = 3.49m, Stock = 55m, IsSoldByWeight = false, Category = "Pastries" }
        };

        foreach (var product in products)
        {
            await _database.InsertAsync(product);
        }
    }

    public async Task<Order> ProcessCheckoutAsync(Order order, List<CartItem> cartItems, int locationId)
    {
        try
        {
            await InitializeAsync();
            
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
            
            if (cartItems == null || cartItems.Count == 0)
            {
                throw new ArgumentException("Cart items cannot be null or empty", nameof(cartItems));
            }
            
            Order resultOrder = null;
            
            await _database.RunInTransactionAsync(conn =>
            {
                // 1. Insert Order
                conn.Insert(order);
                resultOrder = order;
                
                foreach (var cartItem in cartItems)
                {
                    if (cartItem == null) continue;
                    
                    // 2. Create OrderItem
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Name = cartItem.Name,
                        Emoji = cartItem.Emoji,
                        Price = cartItem.Price,
                        Quantity = cartItem.Quantity,
                        IsSoldByWeight = cartItem.IsSoldByWeight
                    };
                    conn.Insert(orderItem);
                    
                    // 3. Update Stock for the specific location
                    var stock = conn.Table<ProductStock>()
                        .Where(ps => ps.ProductId == cartItem.ProductId && ps.LocationId == locationId)
                        .FirstOrDefault();
                        
                    if (stock != null)
                    {
                        stock.Stock -= cartItem.Quantity;
                        if (stock.Stock < 0) stock.Stock = 0;
                        stock.LastUpdated = DateTime.Now;
                        conn.Update(stock);
                    }
                    else
                    {
                        // If no stock record exists, create one with negative (or zero) stock as a fallback
                        // realistically, CheckStockAvailability should prevent this
                        stock = new ProductStock 
                        { 
                            ProductId = cartItem.ProductId, 
                            LocationId = locationId, 
                            Stock = -cartItem.Quantity,
                            LastUpdated = DateTime.Now
                        };
                        conn.Insert(stock);
                        System.Diagnostics.Debug.WriteLine($"Warning: ProductStock record not found for product {cartItem.ProductId} at location {locationId}. Created new record.");
                    }
                }
                
                // 4. Clear Cart
                conn.DeleteAll<CartItem>();
            });
            
            return resultOrder;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing checkout: {ex}");
            throw; // Re-throw to allow caller to handle
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
        
        // Normalize the date to ensure consistent comparison (time part is set to 00:00:00)
        record.Date = record.Date.Date;
        
        // Check if there's an existing record with the same UserId and Date
        var existingRecord = await GetAttendanceRecordAsync(record.UserId, record.Date);
        
        if (existingRecord != null)
        {
            // Update the existing record by setting its Id to the existing record's Id
            // This ensures we update the correct record in the database
            record.Id = existingRecord.Id;
            return await _database.UpdateAsync(record);
        }
        
        // If no existing record, insert new one
        return await _database.InsertAsync(record);
    }

    public async Task<int> DeleteAttendanceRecordAsync(AttendanceRecord record)
    {
        await InitializeAsync();
        return await _database.DeleteAsync(record);
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

    public async Task<bool> CheckStockAvailabilityAsync(int productId, decimal requestedQuantity, int locationId)
    {
        await InitializeAsync();
        var stock = await GetProductStockAsync(productId, locationId);
        return stock != null && stock.Stock >= requestedQuantity;
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
        // Seeding is now handled by the Initial Setup flow (InitialSetupPage/ViewModel).
        // This method is kept for backward compatibility or potential future seeding needs,
        // but it no longer creates insecure default users.
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

        if (!columns.Any(c => string.Equals(c.name, "OvertimeMultiplier", StringComparison.OrdinalIgnoreCase)))
        {
            await _database.ExecuteAsync("ALTER TABLE User ADD COLUMN OvertimeMultiplier REAL NOT NULL DEFAULT 1.5;");
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

        if (!columns.Any(c => string.Equals(c.name, "AbsencePermissionType", StringComparison.OrdinalIgnoreCase)))
        {
            await AddColumnAsync("AbsencePermissionType", "TEXT", "NOT NULL DEFAULT 'None'");
        }
    }

    private async Task EnsureProductTableColumnsAsync()
    {
        var columns = await _database.QueryAsync<TableInfo>("PRAGMA table_info(Product);");

        if (!columns.Any(c => string.Equals(c.name, "Category", StringComparison.OrdinalIgnoreCase)))
        {
            await _database.ExecuteAsync("ALTER TABLE Product ADD COLUMN Category TEXT NOT NULL DEFAULT 'All';");
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
        
        var records = await query.OrderByDescending(r => r.RestockDate).ToListAsync();
        await PopulateUserNamesAsync(records);
        return records;
    }

    public async Task<List<RestockRecord>> GetRestockRecordsByProductAsync(int productId)
    {
        await InitializeAsync();
        var records = await _database.Table<RestockRecord>()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.RestockDate)
            .ToListAsync();
        await PopulateUserNamesAsync(records);
        return records;
    }

    public async Task<List<RestockRecord>> GetRestockRecordsByUserAsync(int userId)
    {
        await InitializeAsync();
        var records = await _database.Table<RestockRecord>()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RestockDate)
            .ToListAsync();
        await PopulateUserNamesAsync(records);
        return records;
    }

    /// <summary>
    /// Populates missing user names in restock records using a batch lookup to avoid N+1 queries.
    /// </summary>
    private async Task PopulateUserNamesAsync(List<RestockRecord> records)
    {
        var userIdsWithMissingNames = records
            .Where(r => string.IsNullOrEmpty(r.UserName) && r.UserId > 0)
            .Select(r => r.UserId)
            .Distinct()
            .ToList();

        if (userIdsWithMissingNames.Any())
        {
            var userLookup = (await _database.Table<User>()
                .Where(u => userIdsWithMissingNames.Contains(u.Id))
                .ToListAsync())
                .ToDictionary(u => u.Id, u => u.Name);

            foreach (var record in records)
            {
                if (string.IsNullOrEmpty(record.UserName) && userLookup.TryGetValue(record.UserId, out var name))
                {
                    record.UserName = name;
                }
            }
        }
    }

    public async Task<int> CreateEmployeeExpenseAsync(EmployeeExpense expense)
    {
        await InitializeAsync();
        return await _database.InsertAsync(expense);
    }

    public async Task<List<EmployeeExpense>> GetEmployeeExpensesAsync(int userId, DateTime? start = null, DateTime? end = null)
    {
        await InitializeAsync();
        var query = _database.Table<EmployeeExpense>().Where(e => e.UserId == userId);
        if (start.HasValue) query = query.Where(e => e.ExpenseDate >= start.Value);
        if (end.HasValue) query = query.Where(e => e.ExpenseDate <= end.Value);
        return await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }

    public async Task<int> DeleteEmployeeExpenseAsync(EmployeeExpense expense)
    {
        await InitializeAsync();
        return await _database.DeleteAsync(expense);
    }

    // ============================================
    // SHOP SETTINGS AND LOCATIONS (Multi-Location)
    // ============================================

    public async Task<ShopSettings> GetShopSettingsAsync()
    {
        await InitializeAsync();
        var settings = await _database.Table<ShopSettings>().FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new ShopSettings();
            await _database.InsertAsync(settings);
        }
        return settings;
    }

    public async Task<int> SaveShopSettingsAsync(ShopSettings settings)
    {
        await InitializeAsync();
        settings.LastModifiedDate = DateTime.Now;
        return await _database.UpdateAsync(settings);
    }

    public async Task<int> UpdateActiveLocationAsync(int locationId)
    {
        await InitializeAsync();
        var settings = await GetShopSettingsAsync();
        settings.ActiveLocationId = locationId;
        return await SaveShopSettingsAsync(settings);
    }

    // ShopLocation CRUD
    public async Task<List<ShopLocation>> GetShopLocationsAsync()
    {
        await InitializeAsync();
        return await _database.Table<ShopLocation>().ToListAsync();
    }

    public async Task<ShopLocation> GetActiveLocationAsync()
    {
        await InitializeAsync();
        var settings = await GetShopSettingsAsync();
        if (settings.ActiveLocationId.HasValue)
        {
            var location = await _database.Table<ShopLocation>().FirstOrDefaultAsync(l => l.Id == settings.ActiveLocationId.Value);
            if (location != null) return location;
        }
        return await _database.Table<ShopLocation>().Where(l => l.IsPrimary).FirstOrDefaultAsync() 
               ?? await _database.Table<ShopLocation>().FirstOrDefaultAsync();
    }

    public async Task<int> CreateShopLocationAsync(ShopLocation location)
    {
        await InitializeAsync();
        var result = await _database.InsertAsync(location);
        // Initialize stock for all existing products at this new location
        await InitializeProductStockForLocationAsync(location.Id);
        return result;
    }

    public async Task<int> UpdateShopLocationAsync(ShopLocation location)
    {
        await InitializeAsync();
        location.LastModifiedDate = DateTime.Now;
        return await _database.UpdateAsync(location);
    }

    public async Task<int> DeleteShopLocationAsync(ShopLocation location)
    {
        await InitializeAsync();
        return await _database.DeleteAsync(location);
    }

    // ProductStock CRUD
    public async Task<ProductStock> GetProductStockAsync(int productId, int locationId)
    {
        await InitializeAsync();
        return await _database.Table<ProductStock>()
            .Where(ps => ps.ProductId == productId && ps.LocationId == locationId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateProductStockAsync(int productId, int locationId, decimal quantityChange)
    {
        await InitializeAsync();
        var stock = await GetProductStockAsync(productId, locationId);
        if (stock == null)
        {
            stock = new ProductStock { ProductId = productId, LocationId = locationId, Stock = 0m };
            await _database.InsertAsync(stock);
        }
        stock.Stock += quantityChange;
        if (stock.Stock < 0m) stock.Stock = 0m;
        stock.LastUpdated = DateTime.Now;
        return await _database.UpdateAsync(stock);
    }

    public async Task<List<ProductStock>> GetLocationStockAsync(int locationId)
    {
        await InitializeAsync();
        return await _database.Table<ProductStock>().Where(ps => ps.LocationId == locationId).ToListAsync();
    }

    /// <summary>
    /// Get products with stock for a specific location, or aggregated stock for all locations.
    /// </summary>
    /// <param name="locationId">Location ID, or null for all locations (aggregated)</param>
    public async Task<List<(Product Product, decimal Stock)>> GetProductsWithStockByLocationAsync(int? locationId)
    {
        await InitializeAsync();
        var products = await _database.Table<Product>().ToListAsync();
        var result = new List<(Product Product, decimal Stock)>();

        if (locationId.HasValue)
        {
            // Single location - get stock from ProductStock table
            var stocks = await _database.Table<ProductStock>()
                .Where(ps => ps.LocationId == locationId.Value)
                .ToListAsync();
            var stockDict = stocks.ToDictionary(s => s.ProductId, s => s.Stock);

            foreach (var product in products)
            {
                var stock = stockDict.TryGetValue(product.Id, out var s) ? s : 0m;
                result.Add((product, stock));
            }
        }
        else
        {
            // All locations - aggregate stock from all ProductStock records
            var allStocks = await _database.Table<ProductStock>().ToListAsync();
            var aggregatedStock = allStocks
                .GroupBy(s => s.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Stock));

            foreach (var product in products)
            {
                var stock = aggregatedStock.TryGetValue(product.Id, out var s) ? s : 0m;
                result.Add((product, stock));
            }
        }

        return result;
    }

    private async Task InitializeProductStockForLocationAsync(int locationId)
    {
        var products = await _database.Table<Product>().ToListAsync();
        foreach (var product in products)
        {
            var exists = await GetProductStockAsync(product.Id, locationId);
            if (exists == null)
            {
                await _database.InsertAsync(new ProductStock
                {
                    ProductId = product.Id,
                    LocationId = locationId,
                    Stock = 0m
                });
            }
        }
    }

    private async Task MigrateProductStockAsync()
    {
        // 1. Ensure at least one location exists
        var locations = await GetShopLocationsAsync();
        if (!locations.Any())
        {
            // If no locations, create a default "Main Branch"
            var mainBranch = new ShopLocation { LocationName = "Main Branch", IsPrimary = true, IsActive = true };
            await _database.InsertAsync(mainBranch);
            
            // Set as active in settings
            var settings = await GetShopSettingsAsync();
            settings.ActiveLocationId = mainBranch.Id;
            await SaveShopSettingsAsync(settings);
            
            locations = new List<ShopLocation> { mainBranch };
        }

        var primaryLocation = locations.FirstOrDefault(l => l.IsPrimary) ?? locations.First();

        // 2. Migrate Product.Stock to ProductStock for primary location
        var products = await _database.Table<Product>().ToListAsync();
        foreach (var product in products)
        {
            var stockRecord = await GetProductStockAsync(product.Id, primaryLocation.Id);
            if (stockRecord == null)
            {
                // Create stock record using current legacy Stock value
                await _database.InsertAsync(new ProductStock
                {
                    ProductId = product.Id,
                    LocationId = primaryLocation.Id,
                    Stock = product.Stock
                });
            }
        }
    }

    // StockTransfer CRUD
    public async Task<int> ExecuteStockTransferAsync(StockTransfer transfer)
    {
        await InitializeAsync();
        int result = 0;
        await _database.RunInTransactionAsync(conn =>
        {
            // Deduct from source
            var source = conn.Table<ProductStock>()
                .Where(ps => ps.ProductId == transfer.ProductId && ps.LocationId == transfer.FromLocationId)
                .FirstOrDefault();
            if (source == null || source.Stock < transfer.Quantity)
                throw new Exception("Insufficient stock at source location");

            source.Stock -= transfer.Quantity;
            conn.Update(source);

            // Add to destination
            var dest = conn.Table<ProductStock>()
                .Where(ps => ps.ProductId == transfer.ProductId && ps.LocationId == transfer.ToLocationId)
                .FirstOrDefault();
            if (dest == null)
            {
                dest = new ProductStock { ProductId = transfer.ProductId, LocationId = transfer.ToLocationId, Stock = 0m };
                conn.Insert(dest);
            }
            dest.Stock += transfer.Quantity;
            conn.Update(dest);

            // Record transfer
            result = conn.Insert(transfer);
        });
        return result;
    }

    public async Task<List<StockTransfer>> GetStockTransfersAsync(int? productId = null, DateTime? start = null, DateTime? end = null)
    {
        await InitializeAsync();
        var query = _database.Table<StockTransfer>();
        if (productId.HasValue) query = query.Where(t => t.ProductId == productId.Value);
        if (start.HasValue) query = query.Where(t => t.TransferDate >= start.Value);
        if (end.HasValue) query = query.Where(t => t.TransferDate <= end.Value);
        return await query.OrderByDescending(t => t.TransferDate).ToListAsync();
    }

    // UserLocation CRUD
    public async Task<List<UserLocation>> GetUserLocationsAsync(int userId)
    {
        await InitializeAsync();
        return await _database.Table<UserLocation>().Where(ul => ul.UserId == userId).ToListAsync();
    }

    public async Task<int> AssignUserToLocationAsync(int userId, int locationId, bool isPrimary)
    {
        await InitializeAsync();
        if (isPrimary)
        {
            // Reset other primary flags for this user
            var existing = await GetUserLocationsAsync(userId);
            foreach (var ul in existing.Where(x => x.IsPrimary))
            {
                ul.IsPrimary = false;
                await _database.UpdateAsync(ul);
            }
        }

        var assignment = await _database.Table<UserLocation>()
            .Where(ul => ul.UserId == userId && ul.LocationId == locationId)
            .FirstOrDefaultAsync();

        if (assignment == null)
        {
            assignment = new UserLocation { UserId = userId, LocationId = locationId, IsPrimary = isPrimary };
            return await _database.InsertAsync(assignment);
        }
        else
        {
            assignment.IsPrimary = isPrimary;
            return await _database.UpdateAsync(assignment);
        }
    }

    public async Task<int> RemoveUserFromLocationAsync(int userId, int locationId)
    {
        await InitializeAsync();
        var assignment = await _database.Table<UserLocation>()
            .Where(ul => ul.UserId == userId && ul.LocationId == locationId)
            .FirstOrDefaultAsync();
        if (assignment != null)
            return await _database.DeleteAsync(assignment);
        return 0;
    }

    public async Task<List<UserLocation>> GetLocationUsersAsync(int locationId)
    {
        await InitializeAsync();
        return await _database.Table<UserLocation>().Where(ul => ul.LocationId == locationId).ToListAsync();
    }

    private class TableInfo
    {
        // ReSharper disable once InconsistentNaming - matches PRAGMA output
        public string name { get; set; }
    }
}
