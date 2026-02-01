# MVVM Basics 08: Best Practices & Common Mistakes

## 🎯 What You'll Learn

By the end of this lesson, you will:
- Learn MVVM best practices
- Avoid common MVVM mistakes
- Write cleaner, more maintainable code
- Understand performance considerations
- Master MVVM naming conventions

---

## 📖 Why Best Practices Matter

Following best practices helps you write code that is:
- ✅ **Easier to understand**
- ✅ **Easier to maintain**
- ✅ **Easier to test**
- ✅ **More performant**
- ✅ **Less buggy**

---

## 🏆 Best Practice 1: Keep ViewModels Focused

### ❌ Bad: ViewModel Does Too Much

```csharp
public class BadViewModel : ObservableObject
{
    // UI logic (should be in View)
    public Color ButtonColor => IsEnabled ? Colors.Blue : Colors.Gray;
    
    // Database logic (should be in Repository/Service)
    public async Task SaveToDatabaseAsync()
    {
        using var connection = new SQLiteConnection("database.db");
        connection.Insert(this);
    }
    
    // Business logic (should be in Model)
    public bool IsValidEmail()
    {
        return Email.Contains("@");
    }
    
    // Navigation logic (should use NavigationService)
    public async Task GoToNextPage()
    {
        await Application.Current.MainPage.Navigation.PushAsync(new NextPage());
    }
}
```

### ✅ Good: ViewModel is Focused

```csharp
public partial class GoodViewModel : ObservableObject
{
    private readonly IUserRepository _userRepository;
    private readonly INavigationService _navigationService;
    private readonly IValidatorService _validator;

    [ObservableProperty]
    private string _email;

    public GoodViewModel(
        IUserRepository userRepository,
        INavigationService navigationService,
        IValidatorService validator)
    {
        _userRepository = userRepository;
        _navigationService = navigationService;
        _validator = validator;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!_validator.IsValidEmail(Email))
        {
            await ShowErrorAsync("Invalid email");
            return;
        }

        await _userRepository.SaveAsync(Email);
    }

    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        await _navigationService.NavigateToAsync("nextpage");
    }
}
```

---

## 🏆 Best Practice 2: Use Proper Naming Conventions

### File Naming

```
✅ Good:
Models/User.cs
ViewModels/UserViewModel.cs
Views/UserPage.xaml
Services/IUserService.cs
Converters/BooleanToColorConverter.cs

❌ Bad:
Models/user.cs
ViewModels/userviewmodel.cs
Views/user.xaml
Services/userservice.cs
```

### Property Naming

```csharp
// ✅ Good: Clear, descriptive names
[ObservableProperty]
private string _userEmailAddress;

[ObservableProperty]
private bool _isUserLoggedIn;

[ObservableProperty]
private int _totalNumberOfItems;

// ❌ Bad: Unclear or abbreviated names
[ObservableProperty]
private string _eml;

[ObservableProperty]
private bool _log;

[ObservableProperty]
private int _cnt;
```

### Command Naming

```csharp
// ✅ Good: Verb + Noun
[RelayCommand]
private async Task LoadUserDataAsync()

[RelayCommand]
private void SaveUserChanges()

[RelayCommand]
private void DeleteSelectedItem()

// ❌ Bad: Unclear or non-descriptive
[RelayCommand]
private async Task DoStuff()

[RelayCommand]
private void HandleIt()

[RelayCommand]
private void Execute()
```

---

## 🏆 Best Practice 3: Handle Async Properly

### ❌ Bad: Blocking the UI

```csharp
// ❌ BAD: Blocks UI thread
[RelayCommand]
private void LoadData()
{
    var data = _service.GetData().Result; // Deadlock risk!
    Data = data;
}

// ❌ BAD: No error handling
[RelayCommand]
private async Task LoadDataAsync()
{
    var data = await _service.GetDataAsync();
    Data = data; // Crashes if service fails!
}
```

### ✅ Good: Proper Async Handling

```csharp
// ✅ GOOD: Proper async with error handling
[RelayCommand]
private async Task LoadDataAsync()
{
    if (IsBusy)
        return;

    try
    {
        IsBusy = true;
        var data = await _service.GetDataAsync();
        Data = data;
    }
    catch (Exception ex)
    {
        await ShowErrorAsync($"Failed to load: {ex.Message}");
    }
    finally
    {
        IsBusy = false;
    }
}
```

---

## 🏆 Best Practice 4: Use IsBusy Pattern

Always show loading state to users.

### Implementation

```csharp
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _isEmpty;

    // Guard clause to prevent multiple simultaneous operations
    protected async Task ExecuteBusyActionAsync(Func<Task> action)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Usage

```csharp
public partial class UsersViewModel : BaseViewModel
{
    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        await ExecuteBusyActionAsync(async () =>
        {
            var users = await _userService.GetUsersAsync();
            Users = new ObservableCollection<User>(users);
            IsEmpty = Users.Count == 0;
        });
    }
}
```

### In View

```xml
<Grid>
    <!-- Content -->
    <CollectionView ItemsSource="{Binding Users}"
                    IsVisible="{Binding IsBusy, Converter={StaticResource InverseBoolConverter}}">
        <!-- Items -->
    </CollectionView>

    <!-- Loading indicator -->
    <ActivityIndicator IsRunning="{Binding IsBusy}"
                       IsVisible="{Binding IsBusy}"
                       HorizontalOptions="Center"
                       VerticalOptions="Center"/>

    <!-- Empty state -->
    <Label Text="No users found"
           IsVisible="{Binding IsEmpty}"
           HorizontalOptions="Center"
           VerticalOptions="Center"
           FontSize="18"
           TextColor="Gray"/>
</Grid>
```

---

## 🏆 Best Practice 5: Validate Input Early

Validate user input before processing.

### ❌ Bad: No Validation

```csharp
[RelayCommand]
private async Task RegisterUserAsync()
{
    // No validation!
    await _userService.RegisterAsync(Email, Password);
}
```

### ✅ Good: Proper Validation

```csharp
[RelayCommand]
private async Task RegisterUserAsync()
{
    // Validate input
    if (string.IsNullOrWhiteSpace(Email))
    {
        await ShowErrorAsync("Please enter your email");
        return;
    }

    if (string.IsNullOrWhiteSpace(Password))
    {
        await ShowErrorAsync("Please enter a password");
        return;
    }

    if (Password.Length < 8)
    {
        await ShowErrorAsync("Password must be at least 8 characters");
        return;
    }

    if (!_emailValidator.IsValid(Email))
    {
        await ShowErrorAsync("Please enter a valid email address");
        return;
    }

    // All validations passed
    await _userService.RegisterAsync(Email, Password);
}
```

### With CanExecute

```csharp
[ObservableProperty]
private string _email;

[ObservableProperty]
private string _password;

[RelayCommand(CanExecute = nameof(CanRegister))]
private async Task RegisterUserAsync()
{
    await _userService.RegisterAsync(Email, Password);
}

private bool CanRegister()
{
    return !string.IsNullOrWhiteSpace(Email) &&
           !string.IsNullOrWhiteSpace(Password) &&
           Password.Length >= 8 &&
           _emailValidator.IsValid(Email);
}

// Notify when properties change
partial void OnEmailChanged(string value)
{
    RegisterCommand.NotifyCanExecuteChanged();
}

partial void OnPasswordChanged(string value)
{
    RegisterCommand.NotifyCanExecuteChanged();
}
```

---

## 🏆 Best Practice 6: Use ObservableCollection Correctly

### ❌ Bad: Clearing and Re-creating

```csharp
// ❌ BAD: Inefficient
[RelayCommand]
private void RefreshData()
{
    Todos.Clear();
    foreach (var item in _dataSource.GetTodos())
    {
        Todos.Add(item);
    }
}
```

### ✅ Good: Replace Entire Collection

```csharp
// ✅ GOOD: More efficient
[RelayCommand]
private void RefreshData()
{
    var newTodos = _dataSource.GetTodos();
    Todos.Clear();
    foreach (var item in newTodos)
    {
        Todos.Add(item);
    }
}

// Even better: Use LINQ
[RelayCommand]
private void RefreshData()
{
    var newTodos = _dataSource.GetTodos();
    Todos = new ObservableCollection<TodoItem>(newTodos);
}
```

---

## 🏆 Best Practice 7: Avoid Memory Leaks

### ❌ Bad: Not Unsubscribing

```csharp
public partial class BadViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    public BadViewModel(IDataService dataService)
    {
        _dataService = dataService;
        
        // ❌ BAD: Never unsubscribes!
        _dataService.DataChanged += OnDataChanged;
    }

    private void OnDataChanged(object sender, DataChangedEventArgs e)
    {
        // Handle data change
    }
}
```

### ✅ Good: Implement IDisposable

```csharp
public partial class GoodViewModel : ObservableObject, IDisposable
{
    private readonly IDataService _dataService;

    public GoodViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _dataService.DataChanged += OnDataChanged;
    }

    private void OnDataChanged(object sender, DataChangedEventArgs e)
    {
        // Handle data change
    }

    public void Dispose()
    {
        _dataService.DataChanged -= OnDataChanged;
    }
}
```

### Or Use WeakReferenceMessenger

```csharp
public partial class BetterViewModel : ObservableObject
{
    public BetterViewModel()
    {
        // ✅ GOOD: Uses weak references, no memory leak
        WeakReferenceMessenger.Default.Register<DataChangedMessage>(this, (r, m) =>
        {
            var viewModel = (BetterViewModel)r;
            viewModel.HandleDataChanged(m);
        });
    }

    private void HandleDataChanged(DataChangedMessage message)
    {
        // Handle data change
    }
}
```

---

## 🏆 Best Practice 8: Use Compiled Bindings

Always use `x:DataType` for better performance and compile-time checking.

### ❌ Bad: No DataType

```xml
<!-- ❌ BAD: Runtime binding, slower, no compile-time errors -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="MyApp.Views.MainPage">
    
    <Label Text="{Binding UserName}" />
    <!-- Typo "UsreName" won't be caught until runtime! -->
    
</ContentPage>
```

### ✅ Good: With DataType

```xml
<!-- ✅ GOOD: Compiled binding, faster, compile-time errors -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.MainPage"
             x:DataType="viewmodels:MainPageViewModel">
    
    <Label Text="{Binding UserName}" />
    <!-- Typo will be caught at compile time! -->
    
</ContentPage>
```

---

## 🚫 Common Mistakes to Avoid

### Mistake 1: Business Logic in View

```csharp
// ❌ BAD: Business logic in code-behind
public partial class BadPage : ContentPage
{
    private void OnSaveClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text;
        var email = EmailEntry.Text;
        
        // Business logic in View!
        if (string.IsNullOrWhiteSpace(name))
        {
            DisplayAlert("Error", "Name is required", "OK");
            return;
        }
        
        if (!email.Contains("@"))
        {
            DisplayAlert("Error", "Invalid email", "OK");
            return;
        }
        
        _userService.Save(name, email);
    }
}
```

### Mistake 2: Direct Model Access from View

```xml
<!-- ❌ BAD: View directly uses Model -->
<ContentPage.BindingContext>
    <models:User />  <!-- Should use ViewModel! -->
</ContentPage.BindingContext>
```

### Mistake 3: Not Using Commands

```csharp
// ❌ BAD: Using event handlers in code-behind
<Button Clicked="OnButtonClicked" />

public void OnButtonClicked(object sender, EventArgs e)
{
    // Logic here
}
```

```xml
<!-- ✅ GOOD: Using Commands -->
<Button Command="{Binding SaveCommand}" />
```

### Mistake 4: Hardcoded Strings

```csharp
// ❌ BAD: Hardcoded strings
await DisplayAlert("Success", "Data saved successfully", "OK");
```

```csharp
// ✅ GOOD: Use resources
await DisplayAlert(
    AppResources.SuccessTitle,
    AppResources.DataSavedMessage,
    AppResources.OKButton);
```

### Mistake 5: Not Handling Nulls

```csharp
// ❌ BAD: No null check
public string FullName => $"{FirstName} {LastName}";
```

```csharp
// ✅ GOOD: Handle nulls
public string FullName => $"{FirstName ?? ""} {LastName ?? ""}".Trim();
```

---

## 🎯 Performance Tips

### 1. Use Lazy Loading

```csharp
public partial class ProductsViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private List<Product> _allProducts;

    [ObservableProperty]
    private ObservableCollection<Product> _displayedProducts;

    public ProductsViewModel(IProductService productService)
    {
        _productService = productService;
        _displayedProducts = new ObservableCollection<Product>();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (_allProducts != null)
            return; // Already loaded

        _allProducts = await _productService.GetProductsAsync();
        DisplayedProducts = new ObservableCollection<Product>(_allProducts);
    }
}
```

### 2. Virtualize Long Lists

```xml
<!-- ✅ GOOD: Uses virtualization for performance -->
<CollectionView ItemsSource="{Binding Items}"
                ItemsLayout="VerticalList">
    <!-- Only renders visible items -->
</CollectionView>

<!-- ❌ BAD: Renders all items at once -->
<StackLayout BindableLayout.ItemsSource="{Binding Items}">
    <BindableLayout.ItemTemplate>
        <DataTemplate>
            <Label Text="{Binding Name}" />
        </DataTemplate>
    </BindableLayout.ItemTemplate>
</StackLayout>
```

### 3. Use DataTemplate Selectors

```xml
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.ItemTemplate>
        <DataTemplateSelector>
            <templates:ItemTemplateSelector />
        </DataTemplateSelector>
    </CollectionView.ItemTemplate>
</CollectionView>
```

---

## ✅ Quick Check

**Question**: Should you put business logic in the View?

<details>
<summary>Answer</summary>

**No!** Business logic belongs in the ViewModel or Model, never in the View.

</details>

---

**Question**: Why use `x:DataType`?

<details>
<summary>Answer</summary>

It enables **compiled bindings** which are faster and catch errors at compile-time instead of runtime.

</details>

---

**Question**: How do you prevent memory leaks with event subscriptions?

<details>
<summary>Answer</summary>

Either implement `IDisposable` and unsubscribe in `Dispose()`, or use `WeakReferenceMessenger` which uses weak references automatically.

</details>

---

## 🎓 Key Takeaways

1. **Keep ViewModels Focused**: One responsibility per ViewModel
2. **Use Proper Naming**: Clear, descriptive names
3. **Handle Async Properly**: Use async/await with error handling
4. **Show Loading State**: Always use IsBusy pattern
5. **Validate Input Early**: Check before processing
6. **Use ObservableCollection**: For collections that change
7. **Avoid Memory Leaks**: Unsubscribe from events
8. **Use Compiled Bindings**: Always set x:DataType

---

## 📚 Next Steps

You've learned the basics! Now continue your journey:

- **Next**: [MVVM Basics 09: Advanced Topics](MVVM_Basics_09_Advanced.md)
- **Practice**: Build more apps using these best practices

---

## 💡 Practice Exercise

**Try This**: Review and refactor your Todo app

**Checklist**:
- [ ] Create a BaseViewModel with IsBusy
- [ ] Add proper error handling to all async methods
- [ ] Use x:DataType in all Views
- [ ] Add input validation
- [ ] Use CanExecute for commands
- [ ] Implement IDisposable if needed
- [ ] Add loading indicators
- [ ] Use proper naming conventions

<details>
<summary>Hint</summary>

Start with the BaseViewModel, then update each ViewModel one by one. Test after each change to make sure nothing breaks.

</details>

---

**Remember**: Best practices are guidelines learned from experience. Follow them, but adapt them to your specific needs!
