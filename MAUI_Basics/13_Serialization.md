# Serialization in .NET MAUI

## 📚 Lesson Overview

**What You'll Learn:**
- What is serialization and why it's important
- Converting C# objects to JSON (serialization)
- Converting JSON to C# objects (deserialization)
- Using System.Text.Json (built-in JSON library)
- Handling complex objects and collections
- Best practices for JSON handling

**Difficulty:** Beginner  
**Time:** 40 minutes  
**Prerequisites:** C# Basics, Classes and Objects (C# Fundamentals Lesson 06)

---

## 🎯 What is Serialization?

**Serialization** is the process of converting an object into a format that can be stored or transmitted.

**Real-world example:** 
- **Object:** `Person { Name: "Ahmed", Age: 25 }`
- **Serialized (JSON):** `{"Name":"Ahmed","Age":25}`
- **Use Case:** Send this data to a web API or save it to a file

### Why Do We Need Serialization?

✅ **Save data** to files or databases  
✅ **Send data** over the internet (APIs)  
✅ **Share data** between different apps  
✅ **Store settings** and preferences  

---

## 🔧 Setting Up JSON Serialization

### Built-in vs Third-Party Libraries

| Library | Pros | Cons |
|---------|------|------|
| **System.Text.Json** | Built-in, fast, modern | Fewer features |
| **Newtonsoft.Json** | Feature-rich, flexible | External dependency |

**Recommendation:** Use **System.Text.Json** for new projects (it's built into .NET).

### No Installation Needed!

`System.Text.Json` is included in .NET MAUI by default. Just add this using statement:

```csharp
using System.Text.Json;
```

---

## 📦 Basic Serialization

### Converting Object to JSON

```csharp
using System.Text.Json;

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

// Create an object
var person = new Person
{
    Name = "Ahmed Mohamed",
    Age = 28,
    Email = "ahmed@example.com"
};

// Serialize to JSON string
string jsonString = JsonSerializer.Serialize(person);

Console.WriteLine(jsonString);
// Output: {"Name":"Ahmed Mohamed","Age":28,"Email":"ahmed@example.com"}
```

### Pretty Print JSON (Formatted)

```csharp
var options = new JsonSerializerOptions
{
    WriteIndented = true // Makes JSON readable
};

string prettyJson = JsonSerializer.Serialize(person, options);

Console.WriteLine(prettyJson);
/* Output:
{
  "Name": "Ahmed Mohamed",
  "Age": 28,
  "Email": "ahmed@example.com"
}
*/
```

---

## 📥 Basic Deserialization

### Converting JSON to Object

```csharp
string jsonString = "{\"Name\":\"Fatma Ali\",\"Age\":32,\"Email\":\"fatma@example.com\"}";

// Deserialize JSON to Person object
Person person = JsonSerializer.Deserialize<Person>(jsonString)!;

Console.WriteLine($"Name: {person.Name}");
Console.WriteLine($"Age: {person.Age}");
Console.WriteLine($"Email: {person.Email}");

// Output:
// Name: Fatma Ali
// Age: 32
// Email: fatma@example.com
```

### Handling Null Values

```csharp
string invalidJson = "invalid json";

// Try to deserialize safely
Person? person = JsonSerializer.Deserialize<Person>(invalidJson);

if (person != null)
{
    Console.WriteLine($"Name: {person.Name}");
}
else
{
    Console.WriteLine("Failed to deserialize JSON");
}
```

---

## 🎨 Working with Collections

### Serializing Lists

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Create a list of products
var products = new List<Product>
{
    new Product { Id = 1, Name = "Laptop", Price = 25000 },
    new Product { Id = 2, Name = "Mouse", Price = 250 },
    new Product { Id = 3, Name = "Keyboard", Price = 500 }
};

// Serialize list to JSON
string json = JsonSerializer.Serialize(products, new JsonSerializerOptions 
{ 
    WriteIndented = true 
});

Console.WriteLine(json);
/* Output:
[
  {"Id":1,"Name":"Laptop","Price":25000},
  {"Id":2,"Name":"Mouse","Price":250},
  {"Id":3,"Name":"Keyboard","Price":500}
]
*/
```

### Deserializing Lists

```csharp
string json = """
[
  {"Id":1,"Name":"Laptop","Price":25000},
  {"Id":2,"Name":"Mouse","Price":250},
  {"Id":3,"Name":"Keyboard","Price":500}
]
""";

// Deserialize JSON to List<Product>
List<Product> products = JsonSerializer.Deserialize<List<Product>>(json)!;

foreach (var product in products)
{
    Console.WriteLine($"{product.Name}: EGP {product.Price}");
}

// Output:
// Laptop: EGP 25000
// Mouse: EGP 250
// Keyboard: EGP 500
```

---

## 🔧 Advanced Serialization Options

### Custom Property Names (CamelCase)

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

string json = JsonSerializer.Serialize(person, options);

// Output: {"name":"Ahmed","age":28,"email":"ahmed@example.com"}
// Note: First letter is lowercase
```

### Ignoring Null Values

```csharp
var options = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var person = new Person
{
    Name = "Ahmed",
    Age = 28,
    Email = null // This will be excluded from JSON
};

string json = JsonSerializer.Serialize(person, options);

// Output: {"Name":"Ahmed","Age":28}
// Email is not included because it's null
```

### Handling Case Sensitivity

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

string json = '{"name":"Ahmed","age":28}'; // lowercase in JSON

// This works even though property names are different case
Person person = JsonSerializer.Deserialize<Person>(json, options)!;
```

---

## 🌐 Real-World Example: API Integration

### Complete Example with REST API

```csharp
using System.Text.Json;
using System.Net.Http.Json;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    
    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.openweathermap.org/");
    }
    
    public async Task<WeatherData?> GetWeatherAsync(string city)
    {
        try
        {
            var apiKey = "YOUR_API_KEY";
            var url = $"data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            
            // This automatically deserializes JSON to WeatherData
            var weatherData = await _httpClient.GetFromJsonAsync<WeatherData>(url);
            
            return weatherData;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            return null;
        }
    }
}

public class WeatherData
{
    [JsonPropertyName("name")] // Maps JSON "name" to C# "City"
    public string City { get; set; } = string.Empty;
    
    [JsonPropertyName("main")]
    public WeatherMain Main { get; set; } = new();
    
    [JsonPropertyName("weather")]
    public List<WeatherDescription> Weather { get; set; } = new();
}

public class WeatherMain
{
    [JsonPropertyName("temp")]
    public decimal Temperature { get; set; }
    
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

public class WeatherDescription
{
    [JsonPropertyName("main")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
```

### Using in ViewModel

```csharp
public partial class WeatherViewModel : ObservableObject
{
    private readonly WeatherService _weatherService;
    
    [ObservableProperty]
    private string _cityName = "Cairo";
    
    [ObservableProperty]
    private string _weatherDisplay = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    public WeatherViewModel(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }
    
    [RelayCommand]
    private async Task LoadWeatherAsync()
    {
        if (IsLoading)
            return;
        
        try
        {
            IsLoading = true;
            
            var weather = await _weatherService.GetWeatherAsync(CityName);
            
            if (weather != null)
            {
                WeatherDisplay = $"{weather.City}: {weather.Main.Temperature}°C, " +
                               $"{weather.Weather[0].Description}";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

## 💾 Saving and Loading JSON Files

### Save Object to File

```csharp
public class JsonFileService
{
    private readonly string _basePath;
    
    public JsonFileService()
    {
        // Use app's data directory
        _basePath = FileSystem.AppDataDirectory;
    }
    
    // Save object to JSON file
    public async Task SaveToFileAsync<T>(T data, string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);
        
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true 
        };
        
        string jsonString = JsonSerializer.Serialize(data, options);
        
        await File.WriteAllTextAsync(filePath, jsonString);
    }
    
    // Load object from JSON file
    public async Task<T?> LoadFromFileAsync<T>(string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);
        
        if (!File.Exists(filePath))
            return default;
        
        string jsonString = await File.ReadAllTextAsync(filePath);
        
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}
```

### Using the File Service

```csharp
public class AppSettings
{
    public string Username { get; set; } = string.Empty;
    public bool NotificationsEnabled { get; set; } = true;
    public string Theme { get; set; } = "Light";
}

// Save settings
var settings = new AppSettings
{
    Username = "Ahmed",
    NotificationsEnabled = true,
    Theme = "Dark"
};

var fileService = new JsonFileService();
await fileService.SaveToFileAsync(settings, "settings.json");

// Load settings
var loadedSettings = await fileService.LoadFromFileAsync<AppSettings>("settings.json");

if (loadedSettings != null)
{
    Console.WriteLine($"Welcome back, {loadedSettings.Username}!");
}
```

---

## 🔄 Combining SQLite and JSON

### Store JSON in SQLite

```csharp
public class CachedData
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string DataType { get; set; } = string.Empty;
    
    // Store JSON as text
    public string JsonContent { get; set; } = string.Empty;
    
    public DateTime CachedAt { get; set; } = DateTime.Now;
}

public class CacheService
{
    private readonly SQLiteAsyncConnection _database;
    
    public CacheService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<CachedData>().Wait();
    }
    
    // Cache API response as JSON
    public async Task CacheDataAsync<T>(string dataType, T data)
    {
        var json = JsonSerializer.Serialize(data);
        
        var cachedData = new CachedData
        {
            DataType = dataType,
            JsonContent = json
        };
        
        await _database.InsertAsync(cachedData);
    }
    
    // Retrieve cached data
    public async Task<T?> GetCachedDataAsync<T>(string dataType)
    {
        var cached = await _database
            .Table<CachedData>()
            .Where(c => c.DataType == dataType)
            .OrderByDescending(c => c.CachedAt)
            .FirstOrDefaultAsync();
        
        if (cached == null)
            return default;
        
        return JsonSerializer.Deserialize<T>(cached.JsonContent);
    }
}
```

---

## ⚠️ Common Mistakes to Avoid

### ❌ BAD: Not handling JSON errors

```csharp
// DON'T DO THIS - Crashes on invalid JSON!
var person = JsonSerializer.Deserialize<Person>(invalidJson); // ❌
Console.WriteLine(person.Name);
```

### ✅ GOOD: Safe deserialization

```csharp
// DO THIS - Handle errors gracefully!
try
{
    var person = JsonSerializer.Deserialize<Person>(jsonString);
    if (person != null)
    {
        Console.WriteLine(person.Name);
    }
}
catch (JsonException ex)
{
    Console.WriteLine($"Invalid JSON: {ex.Message}");
}
```

### ❌ BAD: Not using attributes for different names

```csharp
// JSON has "user_name" but C# has "UserName"
public class User
{
    public string UserName { get; set; } // Won't match JSON!
}
```

### ✅ GOOD: Using JsonPropertyName attribute

```csharp
public class User
{
    [JsonPropertyName("user_name")] // Maps correctly
    public string UserName { get; set; }
}
```

---

## 🎓 Key Takeaways

1. **Serialization** converts objects to JSON for storage/transmission
2. **System.Text.Json** is built-in and recommended for .NET MAUI
3. **Use async/await** when reading/writing files or making API calls
4. **Handle JsonException** when deserializing untrusted JSON
5. **Use JsonPropertyName** attribute when JSON names differ from C# names
6. **Combine with SQLite** to cache API responses for offline support

---

## 🚀 Next Steps

- **Lesson 11:** SQLite Basics - Store serialized data locally
- **Lesson 12:** REST APIs - Fetch JSON from web services
- **Practice:** Build a complete app that fetches data from an API, caches it locally, and displays it

---

## 💡 Practice Exercise

**Build a Currency Converter:**

1. Create a `CurrencyRate` model with Code, Name, and Rate
2. Fetch exchange rates from a free API (e.g., [exchangerate-api.com](https://www.exchangerate-api.com/))
3. Deserialize the JSON response
4. Cache rates in SQLite for offline use
5. Build a UI to convert between currencies

**Sample API Response:**
```json
{
  "base_code": "USD",
  "conversion_rates": {
    "EGP": 48.5,
    "EUR": 0.92,
    "GBP": 0.79
  }
}
```

---

## 🔗 Useful Resources

- **System.Text.Json Docs:** [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- **JSON Validator:** [jsonlint.com](https://jsonlint.com/)
- **Free JSON APIs:** [jsonplaceholder.typicode.com](https://jsonplaceholder.typicode.com/)

---

**Need Help?** Check the official [System.Text.Json documentation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
