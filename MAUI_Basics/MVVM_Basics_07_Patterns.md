# MVVM Basics 07: Common MVVM Patterns

## 🎯 What You'll Learn

By the end of this lesson, you will:
- Learn common MVVM patterns and best practices
- Understand how to handle navigation in MVVM
- Master dependency injection in MVVM
- Learn how to handle messages between ViewModels
- Understand service patterns in MVVM

---

## 📖 Why Learn Patterns?

Patterns are **reusable solutions** to common problems. They help you write better, more maintainable code.

### Benefits of Using Patterns:
✅ **Proven Solutions**: Tested by many developers
✅ **Consistency**: Same approach across your app
✅ **Maintainable**: Easier to understand and modify
✅ **Scalable**: Works for small and large apps

---

## 🏗️ Pattern 1: Base ViewModel

Create a base ViewModel class with common functionality that all ViewModels inherit from.

### Why Use It?
- Avoid repeating code
- Provide common features to all ViewModels
- Ensure consistency

### Implementation:

```csharp
// ViewModels/BaseViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    // Common property for page title
    [ObservableProperty]
    private string _title;

    // Common property for loading state
    [ObservableProperty]
    private bool _isBusy;

    // Common property for error messages
    [ObservableProperty]
    private string _errorMessage;

    // Common method to show errors
    [RelayCommand]
    public async Task ShowErrorAsync(string message)
    {
        await Application.Current?.MainPage?.DisplayAlert("Error", message, "OK");
    }

    // Common method to show alerts
    [RelayCommand]
    public async Task ShowAlertAsync(string title, string message)
    {
        await Application.Current?.MainPage?.DisplayAlert(title, message, "OK");
    }
}
```

### Usage:

```csharp
// ViewModels/MainPageViewModel.cs
public partial class MainPageViewModel : BaseViewModel
{
    // Now you have access to Title, IsBusy, ErrorMessage, etc.
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            // Load data...
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

---

## 🧭 Pattern 2: Navigation in MVVM

Handle navigation from ViewModels without referencing Views directly.

### Why Use It?
- ViewModels shouldn't know about Views
- Testable ViewModels (no UI dependencies)
- Clean separation of concerns

### Implementation:

```csharp
// Services/NavigationService.cs
public interface INavigationService
{
    Task NavigateToAsync(string route, IDictionary<string, object> parameters = null);
    Task GoBackAsync();
}

public class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route, IDictionary<string, object> parameters = null)
    {
        if (parameters != null)
            await Shell.Current.GoToAsync(route, parameters);
        else
            await Shell.Current.GoToAsync(route);
    }

    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
```

### Register in DI Container:

```csharp
// MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .RegisterServices(); // Custom extension method

        return builder.Build();
    }
}

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddTransient<MainPageViewModel>();
        // Register other services...
    }
}
```

### Usage in ViewModel:

```csharp
// ViewModels/MainPageViewModel.cs
public partial class MainPageViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;

    public MainPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task GoToDetailsAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "ItemId", 123 }
        };
        
        await _navigationService.NavigateToAsync("details", parameters);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }
}
```

---

## 📨 Pattern 3: Messaging Between ViewModels

Allow ViewModels to communicate without direct references.

### Why Use It?
- Loose coupling between ViewModels
- One ViewModel can notify others
- Event-driven communication

### Implementation using CommunityToolkit:

```csharp
// ViewModels/SenderViewModel.cs
public partial class SenderViewModel : ObservableObject
{
    [RelayCommand]
    private void SendMessage()
    {
        // Send a message
        WeakReferenceMessenger.Default.Send(new RefreshMessage(true));
    }
}

// Define the message
public record RefreshMessage(bool ShouldRefresh);

// ViewModels/ReceiverViewModel.cs
public partial class ReceiverViewModel : ObservableObject
{
    public ReceiverViewModel()
    {
        // Register to receive messages
        WeakReferenceMessenger.Default.Register<RefreshMessage>(this, (r, m) =>
        {
            var receiver = (ReceiverViewModel)r;
            receiver.HandleRefreshMessage(m);
        });
    }

    private void HandleRefreshMessage(RefreshMessage message)
    {
        if (message.ShouldRefresh)
        {
            // Refresh data
        }
    }
}
```

---

## 🔄 Pattern 4: Async/Await in ViewModels

Properly handle asynchronous operations in ViewModels.

### Why Use It?
- Keep UI responsive
- Handle loading states
- Proper error handling

### Implementation:

```csharp
public partial class DataViewModel : BaseViewModel
{
    [ObservableProperty]
    private List<User> _users;

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        // Guard clause: prevent multiple simultaneous loads
        if (IsBusy)
            return;

        try
        {
            IsBusy = true; // Show loading indicator
            
            // Simulate API call
            await Task.Delay(1000);
            
            // Load data
            Users = await _userService.GetUsersAsync();
        }
        catch (Exception ex)
        {
            // Handle error
            await ShowErrorAsync($"Failed to load users: {ex.Message}");
        }
        finally
        {
            IsBusy = false; // Hide loading indicator
        }
    }

    // Command with CanExecute for async operations
    [RelayCommand(CanExecute = nameof(CanLoadUsers))]
    private async Task LoadUsersAsync()
    {
        // Same implementation as above
    }

    private bool CanLoadUsers() => !IsBusy;
}
```

### In View:

```xml
<ContentPage>
    <Grid RowDefinitions="*,Auto">
        <!-- Show loading indicator when busy -->
        <ActivityIndicator Grid.RowSpan="2"
                           IsRunning="{Binding IsBusy}"
                           IsVisible="{Binding IsBusy}"
                           HorizontalOptions="Center"
                           VerticalOptions="Center"/>
        
        <!-- Content -->
        <CollectionView Grid.Row="0"
                        ItemsSource="{Binding Users}"
                        IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBoolConverter}}">
            <!-- Item template -->
        </CollectionView>
        
        <!-- Refresh button -->
        <Button Grid.Row="1"
                Text="Refresh"
                Command="{Binding LoadUsersCommand}"
                IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBoolConverter}}"/>
    </Grid>
</ContentPage>
```

---

## 🗄️ Pattern 5: Repository Pattern

Separate data access logic from ViewModels.

### Why Use It?
- ViewModels don't know about data sources
- Easy to switch data sources (database, API, etc.)
- Testable ViewModels (can mock repositories)

### Implementation:

```csharp
// Interfaces/IUserRepository.cs
public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

// Data/UserRepository.cs
public class UserRepository : IUserRepository
{
    private readonly DatabaseService _database;

    public UserRepository(DatabaseService database)
    {
        _database = database;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _database.GetUsersAsync();
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _database.GetUserByIdAsync(id);
    }

    public async Task AddAsync(User user)
    {
        await _database.AddUserAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _database.UpdateUserAsync(user);
    }

    public async Task DeleteAsync(int id)
    {
        await _database.DeleteUserAsync(id);
    }
}
```

### Usage in ViewModel:

```csharp
public partial class UsersViewModel : BaseViewModel
{
    private readonly IUserRepository _userRepository;

    public UsersViewModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [ObservableProperty]
    private ObservableCollection<User> _users;

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var users = await _userRepository.GetAllAsync();
            Users = new ObservableCollection<User>(users);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

---

## 🔧 Pattern 6: Dependency Injection

Inject dependencies into ViewModels instead of creating them inside.

### Why Use It?
- Loose coupling
- Easier testing
- Better code organization

### Register Services:

```csharp
// MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });

    // Register services
    builder.Services.AddSingleton<IUserService, UserService>();
    builder.Services.AddSingleton<INavigationService, NavigationService>();
    builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

    // Register ViewModels as transient (new instance each time)
    builder.Services.AddTransient<MainPageViewModel>();
    builder.Services.AddTransient<UsersViewModel>();

    return builder.Build();
}
```

### Inject into ViewModel:

```csharp
public partial class UsersViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;

    // Dependencies are injected automatically
    public UsersViewModel(IUserService userService, INavigationService navigationService)
    {
        _userService = userService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task NavigateToUserDetailAsync(User user)
    {
        await _navigationService.NavigateToAsync("userdetail", user);
    }
}
```

---

## 🎨 Pattern 7: Value Converters

Transform data for display without changing the ViewModel.

### Why Use It?
- Keep ViewModels clean
- Reusable transformations
- Format data for display

### Common Converters:

```csharp
// Converters/InverseBoolConverter.cs
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }
}

// Converters/BooleanToColorConverter.cs
public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Colors.Green : Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converters/ItemCountConverter.cs
public class ItemCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<object> collection)
        {
            return collection.Count == 0 ? "No items" : $"{collection.Count} items";
        }
        return "0 items";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

### Usage in XAML:

```xml
<ContentPage.Resources>
    <ResourceDictionary>
        <converters:InverseBoolConverter x:Key="InverseBool"/>
        <converters:BooleanToColorConverter x:Key="BoolToColor"/>
        <converters:ItemCountConverter x:Key="ItemCount"/>
    </ResourceDictionary>
</ContentPage.Resources>

<!-- Use converters -->
<ActivityIndicator IsRunning="{Binding IsBusy}"/>
<Button Text="Save" 
        IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBool}}"/>
<Label Text="{Binding IsOnline, Converter={StaticResource BoolToColor}}"/>
<Label Text="{Binding Items, Converter={StaticResource ItemCount}}"/>
```

---

## 🎯 Pattern 8: Lazy Loading

Load data only when needed to improve performance.

### Implementation:

```csharp
public partial class ProductsViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private bool _isLoaded = false;

    [ObservableProperty]
    private ObservableCollection<Product> _products;

    public ProductsViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        // Only load if not already loaded
        if (_isLoaded)
            return;

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            Products = new ObservableCollection<Product>(await _productService.GetProductsAsync());
            _isLoaded = true;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Override OnAppearing in code-behind or use message
    public async Task OnAppearingAsync()
    {
        await LoadProductsAsync();
    }
}
```

---

## ✅ Quick Check

**Question**: Why use a base ViewModel?

<details>
<summary>Answer</summary>

To avoid repeating common code across all ViewModels and provide consistent functionality like `IsBusy`, `Title`, and error handling.

</details>

---

**Question**: How do ViewModels navigate without referencing Views?

<details>
<summary>Answer</summary>

By using a `NavigationService` that abstracts the navigation logic. The ViewModel calls the service, and the service handles the actual navigation.

</details>

---

**Question**: What is the Repository pattern used for?

<details>
<summary>Answer</summary>

To separate data access logic from ViewModels, making it easier to switch data sources and test ViewModels.

</details>

---

## 🎓 Key Takeaways

1. **Base ViewModel**: Reuse common functionality
2. **Navigation Service**: Navigate without View references
3. **Messaging**: Communicate between ViewModels loosely
4. **Async/Await**: Handle async operations properly
5. **Repository**: Separate data access logic
6. **Dependency Injection**: Inject dependencies for testability
7. **Value Converters**: Transform data for display
8. **Lazy Loading**: Load data only when needed

---

## 📚 Next Steps

Now that you know common patterns, learn about testing:

- **Next**: [MVVM Basics 08: Testing MVVM Apps](MVVM_Basics_08_Testing.md)
- **Then**: [MVVM Basics 09: Best Practices](MVVM_Basics_09_Best_Practices.md)

---

## 💡 Practice Exercise

**Try This**: Refactor your Todo app to use these patterns

**Requirements**:
1. Create a `BaseViewModel` with `IsBusy` and error handling
2. Create an `ITodoRepository` interface
3. Use dependency injection to inject the repository
4. Add a navigation service to navigate to a detail page

<details>
<summary>Hint</summary>

Start with the BaseViewModel, then create the repository interface, register services in MauiProgram, and finally update the TodoViewModel to use the injected repository.

</details>

---

**Remember**: Patterns are tools, not rules. Use them when they make sense for your app!
