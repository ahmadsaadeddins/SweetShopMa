# REST APIs in .NET MAUI

## 📚 Lesson Overview

**What You'll Learn:**
- What are REST APIs and how they work
- Making HTTP requests (GET, POST, PUT, DELETE)
- Using HttpClient in .NET MAUI
- Handling JSON responses
- Error handling and network issues
- Best practices for API integration

**Difficulty:** Beginner  
**Time:** 50 minutes  
**Prerequisites:** C# Basics, Async/Await (Lesson 04), MVVM Pattern (Lesson 03)

---

## 🎯 What is a REST API?

**REST** (Representational State Transfer) is a way for apps to communicate with servers over the internet.

**Real-world example:** When you open a weather app, it sends a request to a server asking "What's the weather in Cairo?" and the server responds with weather data.

### Common HTTP Methods

| Method | Purpose | Example |
|--------|---------|---------|
| **GET** | Retrieve data | Get all products |
| **POST** | Create new data | Add new user |
| **PUT** | Update existing data | Update product price |
| **DELETE** | Remove data | Delete user account |

---

## 🔧 Setting Up HttpClient

### Step 1: Register HttpClient (Best Practice)

In **MauiProgram.cs**, register HttpClient with dependency injection:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        
        // Register HttpClient with base URL
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://api.example.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddSingleton<ProductViewModel>();
        builder.Services.AddSingleton<ProductsPage>();
        
        return builder.Build();
    }
}
```

### Step 2: Create Data Models

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
```

---

## 📡 Making API Requests

### Basic API Service

```csharp
using System.Text.Json;
using System.Net.Http.Json;

public class ApiService
{
    private readonly HttpClient _httpClient;
    
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    // GET: Retrieve all products
    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("products");
            
            response.EnsureSuccessStatusCode(); // Throws if not 200-299
            
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }
        catch (HttpRequestException ex)
        {
            // Handle network errors
            Console.WriteLine($"Network error: {ex.Message}");
            return new List<Product>();
        }
    }
    
    // GET: Retrieve single product by ID
    public async Task<Product?> GetProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"products/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Product>();
            }
            
            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching product: {ex.Message}");
            return null;
        }
    }
    
    // POST: Create new product
    public async Task<bool> CreateProductAsync(Product product)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("products", product);
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error creating product: {ex.Message}");
            return false;
        }
    }
    
    // PUT: Update existing product
    public async Task<bool> UpdateProductAsync(int id, Product product)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"products/{id}", product);
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error updating product: {ex.Message}");
            return false;
        }
    }
    
    // DELETE: Remove product
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"products/{id}");
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error deleting product: {ex.Message}");
            return false;
        }
    }
}
```

---

## 🎨 Using API in ViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ProductViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    
    [ObservableProperty]
    private ObservableCollection<Product> _products = new();
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    public ProductViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }
    
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (IsLoading)
            return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var products = await _apiService.GetProductsAsync();
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load products: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task RefreshProductsAsync()
    {
        await LoadProductsAsync();
    }
}
```

---

## 🔐 Adding Authentication

Many APIs require authentication. Here's how to add an API key or JWT token:

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "YOUR_API_KEY_HERE";
    
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // Add API key to all requests
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }
    
    // Alternative: Add token per request
    public async Task<List<Product>> GetProductsAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "products");
        request.Headers.Add("Authorization", $"Bearer {token}");
        
        var response = await _httpClient.SendAsync(request);
        // ... rest of code
    }
}
```

---

## 🌐 Handling Network Errors

### Comprehensive Error Handling

```csharp
public async Task<ApiResponse<List<Product>>> GetProductsSafeAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("products");
        
        if (response.IsSuccessStatusCode)
        {
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return new ApiResponse<List<Product>>
            {
                Success = true,
                Data = products ?? new List<Product>()
            };
        }
        else
        {
            // Handle specific status codes
            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new ApiResponse<List<Product>>
                {
                    Success = false,
                    Message = "You're not authorized to access this data"
                },
                System.Net.HttpStatusCode.NotFound => new ApiResponse<List<Product>>
                {
                    Success = false,
                    Message = "Products not found"
                },
                _ => new ApiResponse<List<Product>>
                {
                    Success = false,
                    Message = $"Server error: {response.StatusCode}"
                }
            };
        }
    }
    catch (HttpRequestException ex)
    {
        return new ApiResponse<List<Product>>
        {
            Success = false,
            Message = $"Network error: {ex.Message}"
        };
    }
    catch (TaskCanceledException)
    {
        return new ApiResponse<List<Product>>
        {
            Success = false,
            Message = "Request timed out. Please check your internet connection"
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<List<Product>>
        {
            Success = false,
            Message = $"Unexpected error: {ex.Message}"
        };
    }
}
```

---

## 📊 Complete Example: Weather App

### Weather Service

```csharp
public class WeatherService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "YOUR_OPENWEATHER_API_KEY";
    
    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
    }
    
    public async Task<WeatherData?> GetWeatherAsync(string city)
    {
        try
        {
            var url = $"weather?q={city}&appid={ApiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<WeatherData>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Weather API error: {ex.Message}");
            return null;
        }
    }
}

public class WeatherData
{
    public string Name { get; set; } = string.Empty;
    public Main Main { get; set; } = new();
    public List<Weather> Weather { get; set; } = new();
}

public class Main
{
    public decimal Temp { get; set; }
    public decimal Feels_Like { get; set; }
    public int Humidity { get; set; }
}

public class Weather
{
    public string Main { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

### Weather ViewModel

```csharp
public partial class WeatherViewModel : ObservableObject
{
    private readonly WeatherService _weatherService;
    
    [ObservableProperty]
    private string _cityName = "Cairo";
    
    [ObservableProperty]
    private WeatherData? _currentWeather;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    public WeatherViewModel(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }
    
    [RelayCommand]
    private async Task GetWeatherAsync()
    {
        if (IsLoading)
            return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var weather = await _weatherService.GetWeatherAsync(CityName);
            
            if (weather != null)
            {
                CurrentWeather = weather;
            }
            else
            {
                ErrorMessage = "Could not fetch weather data";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### Weather Page XAML

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.Views.WeatherPage"
             Title="Weather App">
    
    <Grid RowDefinitions="Auto,Auto,Auto,Auto,*" Padding="20">
        
        <!-- Search Box -->
        <Entry Grid.Row="0"
               Placeholder="Enter city name"
               Text="{Binding CityName}"
               Margin="0,0,0,10"/>
        
        <!-- Get Weather Button -->
        <Button Grid.Row="1"
                Text="Get Weather"
                Command="{Binding GetWeatherCommand}"
                IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBoolConverter}}"
                Margin="0,0,0,20"/>
        
        <!-- Loading Indicator -->
        <ActivityIndicator Grid.Row="2"
                           IsRunning="{Binding IsLoading}"
                           IsVisible="{Binding IsLoading}"/>
        
        <!-- Error Message -->
        <Label Grid.Row="3"
               Text="{Binding ErrorMessage}"
               TextColor="Red"
               IsVisible="{Binding ErrorMessage, Converter={StaticResource NotEmptyConverter}}"/>
        
        <!-- Weather Display -->
        <VerticalStackLayout Grid.Row="4"
                             IsVisible="{Binding CurrentWeather, Converter={StaticResource NotNullConverter}}"
                             Spacing="15">
            
            <Label Text="{Binding CurrentWeather.Name}"
                   FontSize="28"
                   FontAttributes="Bold"/>
            
            <Label Text="{Binding CurrentWeather.Main.Temp, StringFormat='Temperature: {0}°C'}"
                   FontSize="24"/>
            
            <Label Text="{Binding CurrentWeather.Main.Humidity, StringFormat='Humidity: {0}%'}"
                   FontSize="18"/>
            
            <Label Text="{Binding CurrentWeather.Weather[0].Description}"
                   FontSize="18"
                   TextTransform="Capitalize"/>
            
        </VerticalStackLayout>
        
    </Grid>
</ContentPage>
```

---

## ⚠️ Common Mistakes to Avoid

### ❌ BAD: Blocking UI with .Result

```csharp
// DON'T DO THIS - Freezes the app!
public void LoadProducts()
{
    var products = _apiService.GetProductsAsync().Result; // ❌
    Products = new ObservableCollection<Product>(products);
}
```

### ✅ GOOD: Using async/await

```csharp
// DO THIS - Keeps app responsive!
public async Task LoadProductsAsync()
{
    var products = await _apiService.GetProductsAsync(); // ✅
    Products = new ObservableCollection<Product>(products);
}
```

### ❌ BAD: Not handling network errors

```csharp
// DON'T DO THIS - Crashes when offline!
public async Task<List<Product>> GetProductsAsync()
{
    var response = await _httpClient.GetAsync("products");
    return await response.Content.ReadFromJsonAsync<List<Product>>(); // ❌
}
```

### ✅ GOOD: Proper error handling

```csharp
// DO THIS - Handles errors gracefully!
public async Task<List<Product>> GetProductsAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("products");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Product>>() 
               ?? new List<Product>();
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Network error: {ex.Message}");
        return new List<Product>();
    }
}
```

---

## 🎓 Key Takeaways

1. **HttpClient** should be registered with DI and reused (not created for each request)
2. **Always use async/await** for network calls to keep UI responsive
3. **Handle errors** - Networks can fail, users may be offline
4. **Use EnsureSuccessStatusCode()** to catch HTTP errors
5. **Display loading indicators** to show the app is working
6. **Cache responses** when possible to reduce API calls

---

## 🚀 Next Steps

- **Lesson 13:** Serialization - Deep dive into JSON conversion
- **Lesson 08:** Local Storage - Combine APIs with SQLite for offline support

---

## 💡 Practice Exercise

**Build a Movie Search App:**

1. Use [The Movie DB API](https://www.themoviedb.org/) (free API key available)
2. Create a search box to find movies by title
3. Display movie posters, titles, and release dates
4. Add error handling for when the API is unavailable

**API Endpoint Example:**
```
GET https://api.themoviedb.org/3/search/movie?api_key=YOUR_KEY&query=avatar
```

---

## 🔗 Useful Resources

- **REST API Tester:** [Postman](https://www.postman.com/)
- **Free APIs:** [Public APIs](https://public-apis.io/)
- **HTTP Status Codes:** [MDN Web Docs](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)

---

**Need Help?** Check the official [HttpClient documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient)
