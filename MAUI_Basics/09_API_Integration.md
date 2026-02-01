# Lesson 9: API Integration

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand how to make HTTP requests in MAUI
- Learn to consume REST APIs
- Master JSON serialization and deserialization
- Handle errors and network issues
- Implement authentication and authorization
- Build offline-first apps with local caching

---

## 📖 HTTP Client in MAUI

MAUI uses `HttpClient` for making HTTP requests to web services.

### Why Use IHttpClientFactory?

❌ **Bad: Creating new HttpClient instances**
```csharp
// DON'T DO THIS - Causes socket exhaustion
public class MyService
{
    public async Task GetData()
    {
        var client = new HttpClient(); // New instance each time!
        var response = await client.GetAsync("https://api.example.com/data");
    }
}
```

✅ **Good: Using IHttpClientFactory**
```csharp
// DO THIS - Reuses connections efficiently
public class MyService
{
    private readonly HttpClient _httpClient;
    
    public MyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task GetData()
    {
        var response = await _httpClient.GetAsync("https://api.example.com/data");
    }
}
```

---

## 🔧 Setting Up HTTP Client

### Step 1: Register HttpClient in MauiProgram.cs

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts => { /* ... */ });
    
    // Register HttpClient
    builder.Services.AddHttpClient<IApiService, ApiService>(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com");
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
    
    return builder.Build();
}
```

### Step 2: Create API Service

```csharp
// Services/IApiService.cs
public interface IApiService
{
    Task<List<Todo>> GetTodosAsync();
    Task<Todo> GetTodoAsync(int id);
    Task<Todo> CreateTodoAsync(Todo todo);
    Task<Todo> UpdateTodoAsync(int id, Todo todo);
    Task DeleteTodoAsync(int id);
}

// Services/ApiService.cs
using System.Net.Http.Json;
using System.Text.Json;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<Todo>> GetTodosAsync()
    {
        var response = await _httpClient.GetAsync("todos");
        
        response.EnsureSuccessStatusCode();
        
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>();
        return todos ?? new List<Todo>();
    }
    
    public async Task<Todo> GetTodoAsync(int id)
    {
        var response = await _httpClient.GetAsync($"todos/{id}");
        
        response.EnsureSuccessStatusCode();
        
        var todo = await response.Content.ReadFromJsonAsync<Todo>();
        return todo;
    }
    
    public async Task<Todo> CreateTodoAsync(Todo todo)
    {
        var response = await _httpClient.PostAsJsonAsync("todos", todo);
        
        response.EnsureSuccessStatusCode();
        
        var createdTodo = await response.Content.ReadFromJsonAsync<Todo>();
        return createdTodo;
    }
    
    public async Task<Todo> UpdateTodoAsync(int id, Todo todo)
    {
        var response = await _httpClient.PutAsJsonAsync($"todos/{id}", todo);
        
        response.EnsureSuccessStatusCode();
        
        var updatedTodo = await response.Content.ReadFromJsonAsync<Todo>();
        return updatedTodo;
    }
    
    public async Task DeleteTodoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"todos/{id}");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## 📦 JSON Serialization

MAUI uses `System.Text.Json` for JSON serialization.

### Model Classes

```csharp
// Models/Todo.cs
public class Todo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
    
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
}
```

### Custom JSON Options

```csharp
// In MauiProgram.cs
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.ConfigureHttpMessageHandlerBuilder(builder =>
{
    // Configure JSON options
    builder.PrimaryHandler = new System.Net.Http.SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    };
});

// Or configure in ApiService constructor
public ApiService(HttpClient httpClient)
{
    _httpClient = httpClient;
    
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
```

---

## 🔐 Authentication

### API Key Authentication

```csharp
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "your_api_key_here";
    
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
    }
}
```

### Bearer Token Authentication

```csharp
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorageService _secureStorage;
    
    public ApiService(HttpClient httpClient, ISecureStorageService secureStorage)
    {
        _httpClient = httpClient;
        _secureStorage = secureStorage;
    }
    
    private async Task SetAuthHeaderAsync()
    {
        var token = await _secureStorage.GetAsync("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
    
    public async Task<List<Todo>> GetTodosAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("todos");
        // ...
    }
}
```

### OAuth 2.0 Flow

```csharp
public class AuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenUrl = "https://auth.example.com/oauth/token";
    
    public async Task<string> GetAccessTokenAsync(string username, string password)
    {
        var request = new
        {
            grant_type = "password",
            username = username,
            password = password,
            client_id = "your_client_id",
            client_secret = "your_client_secret"
        };
        
        var response = await _httpClient.PostAsJsonAsync(TokenUrl, request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return result.AccessToken;
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
```

---

## 🌐 Error Handling

### Try-Catch Pattern

```csharp
public async Task<List<Todo>> GetTodosAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("todos");
        response.EnsureSuccessStatusCode();
        
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>();
        return todos ?? new List<Todo>();
    }
    catch (HttpRequestException ex)
    {
        // Network error
        Console.WriteLine($"Network error: {ex.Message}");
        return new List<Todo>();
    }
    catch (TaskCanceledException ex)
    {
        // Timeout
        Console.WriteLine($"Request timed out: {ex.Message}");
        return new List<Todo>();
    }
    catch (JsonException ex)
    {
        // JSON parsing error
        Console.WriteLine($"JSON error: {ex.Message}");
        return new List<Todo>();
    }
}
```

### Custom Error Handling Service

```csharp
// Services/IErrorHandler.cs
public interface IErrorHandler
{
    Task<ApiResult<T>> TryExecuteAsync<T>(Func<Task<T>> func);
}

// Services/ErrorHandler.cs
public class ErrorHandler : IErrorHandler
{
    public async Task<ApiResult<T>> TryExecuteAsync<T>(Func<Task<T>> func)
    {
        try
        {
            var result = await func();
            return ApiResult<T>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure("Network error. Please check your connection.");
        }
        catch (TaskCanceledException ex)
        {
            return ApiResult<T>.Failure("Request timed out. Please try again.");
        }
        catch (JsonException ex)
        {
            return ApiResult<T>.Failure("Data format error. Please contact support.");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure("An unexpected error occurred.");
        }
    }
}

// Models/ApiResult.cs
public class ApiResult<T>
{
    public bool IsSuccess { get; private set; }
    public T Data { get; private set; }
    public string ErrorMessage { get; private set; }
    
    public static ApiResult<T> Success(T data)
    {
        return new ApiResult<T> { IsSuccess = true, Data = data };
    }
    
    public static ApiResult<T> Failure(string errorMessage)
    {
        return new ApiResult<T> { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
```

### Using Error Handler

```csharp
public class TodoListViewModel
{
    private readonly IApiService _api;
    private readonly IErrorHandler _errorHandler;
    
    public TodoListViewModel(IApiService api, IErrorHandler errorHandler)
    {
        _api = api;
        _errorHandler = errorHandler;
    }
    
    private async Task LoadTodosAsync()
    {
        IsBusy = true;
        
        var result = await _errorHandler.TryExecuteAsync(() => _api.GetTodosAsync());
        
        if (result.IsSuccess)
        {
            Todos.Clear();
            foreach (var todo in result.Data)
            {
                Todos.Add(todo);
            }
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
        }
        
        IsBusy = false;
    }
}
```

---

## 📡 Using Refit (Type-Safe HTTP Client)

Refit makes API calls easier with automatic interface implementation.

### Step 1: Install Refit

```bash
dotnet add package Refit
```

### Step 2: Define API Interface

```csharp
using Refit;

// Interfaces/ITodoApi.cs
public interface ITodoApi
{
    [Get("/todos")]
    Task<List<Todo>> GetTodosAsync();
    
    [Get("/todos/{id}")]
    Task<Todo> GetTodoAsync(int id);
    
    [Post("/todos")]
    Task<Todo> CreateTodoAsync([Body] Todo todo);
    
    [Put("/todos/{id}")]
    Task<Todo> UpdateTodoAsync(int id, [Body] Todo todo);
    
    [Delete("/todos/{id}")]
    Task DeleteTodoAsync(int id);
}
```

### Step 3: Register in MauiProgram.cs

```csharp
// In MauiProgram.cs
builder.Services.AddRefitClient<ITodoApi>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
```

### Step 4: Use in ViewModel

```csharp
public class TodoListViewModel
{
    private readonly ITodoApi _api;
    
    public TodoListViewModel(ITodoApi api)
    {
        _api = api;
    }
    
    private async Task LoadTodosAsync()
    {
        var todos = await _api.GetTodosAsync();
        // ...
    }
}
```

---

## 🔄 Offline-First Architecture

Build apps that work without internet connection.

### Offline Service Pattern

```csharp
// Services/IOfflineTodoService.cs
public interface IOfflineTodoService
{
    Task<List<Todo>> GetTodosAsync();
    Task<Todo> GetTodoAsync(int id);
    Task<Todo> CreateTodoAsync(Todo todo);
    Task SyncAsync();
}

// Services/OfflineTodoService.cs
public class OfflineTodoService : IOfflineTodoService
{
    private readonly IApiService _api;
    private readonly IDatabaseService _database;
    private readonly INetworkService _network;
    
    public OfflineTodoService(
        IApiService api,
        IDatabaseService database,
        INetworkService network)
    {
        _api = api;
        _database = database;
        _network = network;
    }
    
    public async Task<List<Todo>> GetTodosAsync()
    {
        if (_network.IsConnected)
        {
            // Fetch from API
            var todos = await _api.GetTodosAsync();
            
            // Update local database
            foreach (var todo in todos)
            {
                await _database.SaveTodoAsync(todo);
            }
            
            return todos;
        }
        else
        {
            // Load from local database
            return await _database.GetTodosAsync();
        }
    }
    
    public async Task<Todo> CreateTodoAsync(Todo todo)
    {
        if (_network.IsConnected)
        {
            // Create on server
            var createdTodo = await _api.CreateTodoAsync(todo);
            
            // Save locally
            await _database.SaveTodoAsync(createdTodo);
            
            return createdTodo;
        }
        else
        {
            // Mark for sync
            todo.NeedsSync = true;
            var savedTodo = await _database.SaveTodoAsync(todo);
            return savedTodo;
        }
    }
    
    public async Task SyncAsync()
    {
        if (!_network.IsConnected)
            return;
        
        // Get pending changes
        var pendingTodos = await _database.GetPendingSyncTodosAsync();
        
        foreach (var todo in pendingTodos)
        {
            if (todo.NeedsSync)
            {
                var syncedTodo = await _api.CreateTodoAsync(todo);
                syncedTodo.NeedsSync = false;
                await _database.SaveTodoAsync(syncedTodo);
            }
        }
    }
}
```

### Network Service

```csharp
// Services/INetworkService.cs
public interface INetworkService
{
    bool IsConnected { get; }
    event EventHandler<bool> ConnectivityChanged;
}

// Platforms/Android/NetworkService.cs
using Android.Net;
using Android.Content;

public class NetworkService : INetworkService
{
    private readonly ConnectivityManager _connectivityManager;
    
    public NetworkService()
    {
        _connectivityManager = (ConnectivityManager)Android.App.Application.Context
            .GetSystemService(Context.ConnectivityService);
    }
    
    public bool IsConnected
    {
        get
        {
            var networkInfo = _connectivityManager.ActiveNetworkInfo;
            return networkInfo != null && networkInfo.IsConnected;
        }
    }
    
    public event EventHandler<bool> ConnectivityChanged;
}
```

---

## 🎯 Complete Example: Weather App

Let's build a weather app that consumes a real API.

### API Interface (Refit)

```csharp
using Refit;

public interface IWeatherApi
{
    [Get("/weather")]
    Task<WeatherResponse> GetWeatherAsync(
        [Query] double lat,
        [Query] double lon,
        [Query] string appid);
}

public class WeatherResponse
{
    [JsonPropertyName("main")]
    public WeatherMain Main { get; set; }
    
    [JsonPropertyName("weather")]
    public List<WeatherCondition> Weather { get; set; }
    
    [JsonPropertyName("name")]
    public string CityName { get; set; }
}

public class WeatherMain
{
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }
    
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
    
    [JsonPropertyName("pressure")]
    public int Pressure { get; set; }
}

public class WeatherCondition
{
    [JsonPropertyName("main")]
    public string Main { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; }
}
```

### Weather Service

```csharp
public class WeatherService
{
    private readonly IWeatherApi _api;
    private const string ApiKey = "your_openweathermap_api_key";
    
    public WeatherService(IWeatherApi api)
    {
        _api = api;
    }
    
    public async Task<WeatherResponse> GetWeatherAsync(double lat, double lon)
    {
        try
        {
            var weather = await _api.GetWeatherAsync(lat, lon, ApiKey);
            return weather;
        }
        catch (HttpRequestException)
        {
            throw new Exception("Network error. Please check your connection.");
        }
        catch (Exception)
        {
            throw new Exception("Failed to fetch weather data.");
        }
    }
}
```

### ViewModel

```csharp
public class WeatherViewModel : ObservableObject
{
    private readonly WeatherService _weatherService;
    private readonly IGeolocation _geolocation;
    
    [ObservableProperty]
    private string _cityName;
    
    [ObservableProperty]
    private double _temperature;
    
    [ObservableProperty]
    private string _description;
    
    [ObservableProperty]
    private int _humidity;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage;
    
    public WeatherViewModel(WeatherService weatherService, IGeolocation geolocation)
    {
        _weatherService = weatherService;
        _geolocation = geolocation;
    }
    
    [RelayCommand]
    private async Task LoadWeatherAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        
        try
        {
            var location = await _geolocation.GetLastKnownLocationAsync();
            
            if (location == null)
            {
                ErrorMessage = "Unable to get location. Please enable location services.";
                return;
            }
            
            var weather = await _weatherService.GetWeatherAsync(
                location.Latitude, 
                location.Longitude);
            
            CityName = weather.CityName;
            Temperature = weather.Main.Temperature;
            Description = weather.Weather.FirstOrDefault()?.Description ?? "Unknown";
            Humidity = weather.Main.Humidity;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Movie Search App

Create a movie search app using OMDb API:

**API**: http://www.omdbapi.com/

**Requirements:**
- Search movies by title
- Display movie details
- Add to favorites (save locally)
- View favorites offline
- Handle API errors gracefully

### Exercise 2: News Reader

Create a news reader app:

**API**: NewsAPI.org (free tier available)

**Requirements:**
- Fetch top headlines
- Filter by category
- Save articles for offline reading
- Refresh on pull-to-refresh
- Show loading indicator

### Exercise 3: GitHub Profile Viewer

Create a GitHub profile viewer:

**API**: https://api.github.com/

**Requirements:**
- Search users
- View user profile
- View user repositories
- View repository details
- Cache responses locally

---

## ❓ Common Beginner Questions

### Q: How do I handle API versioning?

**A**: Include version in URL or header:
```csharp
client.BaseAddress = new Uri("https://api.example.com/v2/");
// or
client.DefaultRequestHeaders.Add("API-Version", "2");
```

### Q: What about rate limiting?

**A**: 
1. Check rate limit headers
2. Implement exponential backoff
3. Cache responses
4. Use pagination

### Q: How do I cancel ongoing requests?

**A**: Use `CancellationToken`:
```csharp
public async Task<List<Todo>> GetTodosAsync(CancellationToken cancellationToken = default)
{
    var response = await _httpClient.GetAsync("todos", cancellationToken);
    // ...
}
```

### Q: Should I use REST or GraphQL?

**A**: 
- **REST**: Simple, widely supported, good for most apps
- **GraphQL**: Flexible, reduces over-fetching, good for complex data

### Q: How do I secure my API keys?

**A**: 
1. Never hardcode in client app
2. Use proxy server (recommended)
3. Use Azure Key Vault or similar
4. Environment variables for development

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **HttpClient** | Make HTTP requests |
| **IHttpClientFactory** | Manage HttpClient lifetimes |
| **System.Text.Json** | Serialize/deserialize JSON |
| **Refit** | Type-safe HTTP client |
| **Bearer Token** | Authentication method |
| **Offline-First** | Work without internet |

---

## ✅ Checklist

Before moving to Lesson 10, make sure you can:

- [ ] Register and use HttpClient properly
- [ ] Make GET, POST, PUT, DELETE requests
- [ ] Serialize and deserialize JSON
- [ ] Handle authentication (API key, Bearer token)
- [ ] Handle errors gracefully
- [ ] Implement offline-first architecture
- [ ] Use Refit for type-safe APIs

---

## 🚀 Next Steps

Great job mastering API integration! In the final lesson, we'll learn about **Deployment** and how to publish your app to app stores.

**Next Lesson**: [Lesson 10: Deployment](10_Deployment.md)

---

## 📖 Additional Reading

- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [Refit Documentation](https://github.com/reactiveui/refit)
- [REST API Best Practices](https://restfulapi.net/)
