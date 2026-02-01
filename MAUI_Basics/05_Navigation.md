# Lesson 5: Navigation

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand different navigation patterns in MAUI
- Learn Shell-based navigation (recommended)
- Master modal navigation
- Pass data between pages
- Handle navigation events
- Implement tabbed and flyout navigation

---

## 📖 Navigation Overview

Navigation allows users to move between different screens (pages) in your app. MAUI provides several navigation patterns, with **Shell** being the modern, recommended approach.

### Navigation Patterns

| Pattern | Use Case | Example |
|---------|----------|---------|
| **Shell Navigation** | Most apps with structured navigation | E-commerce, social apps |
| **Modal Navigation** | Temporary screens, forms | Login, details, settings |
| **Tabbed Navigation** | Parallel content sections | News categories, profile tabs |
| **Flyout Navigation** | Apps with drawer menu | Settings, main menu options |

---

## 🐚 Shell Navigation (Recommended)

Shell is MAUI's modern navigation system that provides a consistent experience across platforms.

### What is Shell?

Shell is a container that manages your app's navigation structure. It provides:
- **Flyout** - Side menu (hamburger menu)
- **Tabs** - Bottom tabs (iOS/Android) or top tabs (Windows)
- **Navigation Stack** - Push/pop pages

### Setting Up Shell

#### Step 1: Create AppShell.xaml

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:MyApp.Views"
       x:Class="MyApp.AppShell"
       FlyoutBehavior="Disabled">

    <!-- Tab Bar -->
    <TabBar>
        <Tab Title="Home" Icon="home.png">
            <ShellContent ContentTemplate="{DataTemplate views:HomePage}" />
        </Tab>
        
        <Tab Title="Search" Icon="search.png">
            <ShellContent ContentTemplate="{DataTemplate views:SearchPage}" />
        </Tab>
        
        <Tab Title="Profile" Icon="profile.png">
            <ShellContent ContentTemplate="{DataTemplate views:ProfilePage}" />
        </Tab>
    </TabBar>

</Shell>
```

#### Step 2: Set Shell as App Root

In `App.xaml.cs`:

```csharp
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
```

### Shell Navigation Methods

#### Navigate to Page

```csharp
// Navigate to registered route
await Shell.Current.GoToAsync("profile");

// Navigate with absolute path
await Shell.Current.GoToAsync("//profile");

// Navigate with parameters
await Shell.Current.GoToAsync("details?itemId=123");
```

#### Go Back

```csharp
// Go back to previous page
await Shell.Current.GoToAsync("..");

// Go back multiple levels
await Shell.Current.GoToAsync("../..");
```

### Registering Routes

Register routes in `AppShell.xaml.cs`:

```csharp
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes
        Routing.RegisterRoute("home", typeof(HomePage));
        Routing.RegisterRoute("details", typeof(DetailsPage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}
```

### Passing Data Between Pages

#### Send Simple Data

```csharp
// From sender page
await Shell.Current.GoToAsync("details?itemId=123&itemName=MyItem");
```

```csharp
// In receiving page (DetailsPage.xaml.cs)
[QueryProperty(nameof(ItemId), "itemId")]
[QueryProperty(nameof(ItemName), "itemName")]
public partial class DetailsPage : ContentPage
{
    private string _itemId;
    public string ItemId
    {
        get => _itemId;
        set
        {
            _itemId = value;
            LoadItem(value);
        }
    }

    private string _itemName;
    public string ItemName
    {
        get => _itemName;
        set
        {
            _itemName = value;
            Title = value;
        }
    }

    private void LoadItem(string id)
    {
        // Load item details
    }
}
```

#### Send Complex Objects

```csharp
// From sender page
var item = new TodoItem { Id = 1, Title = "Buy groceries" };
var navigationParameter = new Dictionary<string, object>
{
    { "Item", item }
};
await Shell.Current.GoToAsync("details", navigationParameter);
```

```csharp
// In receiving page
[QueryProperty(nameof(Item), "Item")]
public partial class DetailsPage : ContentPage
{
    private TodoItem _item;
    public TodoItem Item
    {
        get => _item;
        set
        {
            _item = value;
            // Use the item
        }
    }
}
```

### Shell Structure Examples

#### Tab Bar Navigation

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:MyApp.Views"
       x:Class="MyApp.AppShell">

    <TabBar>
        <Tab Title="Home" Icon="home.png">
            <ShellContent ContentTemplate="{DataTemplate views:HomePage}" />
        </Tab>
        
        <Tab Title="Products" Icon="products.png">
            <ShellContent ContentTemplate="{DataTemplate views:ProductsPage}" />
        </Tab>
        
        <Tab Title="Cart" Icon="cart.png">
            <ShellContent ContentTemplate="{DataTemplate views:CartPage}" />
        </Tab>
    </TabBar>

</Shell>
```

#### Flyout Navigation

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:MyApp.Views"
       x:Class="MyApp.AppShell"
       FlyoutBehavior="Flyout">

    <!-- Flyout Header -->
    <Shell.FlyoutHeader>
        <Grid BackgroundColor="#2196F3" HeightRequest="200">
            <Label Text="My App"
                   TextColor="White"
                   FontSize="24"
                   HorizontalOptions="Center"
                   VerticalOptions="Center" />
        </Grid>
    </Shell.FlyoutHeader>

    <!-- Flyout Items -->
    <FlyoutItem Title="Home" Icon="home.png">
        <ShellContent ContentTemplate="{DataTemplate views:HomePage}" />
    </FlyoutItem>
    
    <FlyoutItem Title="Products" Icon="products.png">
        <ShellContent ContentTemplate="{DataTemplate views:ProductsPage}" />
    </FlyoutItem>
    
    <FlyoutItem Title="Settings" Icon="settings.png">
        <ShellContent ContentTemplate="{DataTemplate views:SettingsPage}" />
    </FlyoutItem>

    <!-- Flyout Footer -->
    <Shell.FlyoutFooter>
        <StackLayout Padding="10" BackgroundColor="#E0E0E0">
            <Label Text="Version 1.0"
                   HorizontalOptions="Center" />
        </StackLayout>
    </Shell.FlyoutFooter>

</Shell>
```

#### Combined: Flyout + Tabs

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:MyApp.Views"
       x:Class="MyApp.AppShell"
       FlyoutBehavior="Flyout">

    <FlyoutItem Title="Shop" Icon="shop.png">
        <TabBar>
            <Tab Title="Products" Icon="products.png">
                <ShellContent ContentTemplate="{DataTemplate views:ProductsPage}" />
            </Tab>
            <Tab Title="Categories" Icon="categories.png">
                <ShellContent ContentTemplate="{DataTemplate views:CategoriesPage}" />
            </Tab>
        </TabBar>
    </FlyoutItem>
    
    <FlyoutItem Title="Account" Icon="account.png">
        <ShellContent ContentTemplate="{DataTemplate views:AccountPage}" />
    </FlyoutItem>

</Shell>
```

---

## 🪟 Modal Navigation

Modal navigation presents a page that requires user action before returning.

### Push Modal Page

```csharp
// Push modal page
await Navigation.PushModalAsync(new SettingsPage());
```

### Pop Modal Page

```csharp
// Pop modal page
await Navigation.PopModalAsync();
```

### Return Data from Modal

**Sender Page:**
```csharp
private async Task ShowSettingsAsync()
{
    var settingsPage = new SettingsPage();
    settingsPage.SettingsSaved += (sender, result) =>
    {
        // Handle result
        ApplySettings(result);
    };
    
    await Navigation.PushModalAsync(settingsPage);
}
```

**Modal Page:**
```csharp
public partial class SettingsPage : ContentPage
{
    public event EventHandler<SettingsResult> SettingsSaved;

    private async Task OnSaveClicked()
    {
        var result = new SettingsResult { /* ... */ };
        SettingsSaved?.Invoke(this, result);
        await Navigation.PopModalAsync();
    }
}
```

---

## 📱 Navigation Events

Handle page lifecycle events.

### OnAppearing

Called when page is about to appear:

```csharp
protected override void OnAppearing()
{
    base.OnAppearing();
    
    // Refresh data
    LoadData();
}
```

### OnDisappearing

Called when page is about to disappear:

```csharp
protected override void OnDisappearing()
{
    base.OnDisappearing();
    
    // Save state
    SaveCurrentState();
}
```

### NavigatingTo

Called before navigation:

```csharp
protected override void OnNavigatingTo(NavigatingToEventArgs e)
{
    base.OnNavigatingTo(e);
    
    // Prepare for navigation
}
```

### NavigatedTo

Called after navigation completes:

```csharp
protected override void OnNavigatedTo(NavigatedToEventArgs e)
{
    base.OnNavigatedTo(e);
    
    // Navigation complete, update UI
}
```

---

## 🎯 Complete Example: Todo App Navigation

Let's build a complete navigation system for a todo app.

### AppShell.xaml

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:TodoApp.Views"
       x:Class="TodoApp.AppShell"
       FlyoutBehavior="Flyout">

    <!-- Flyout Header -->
    <Shell.FlyoutHeader>
        <Grid BackgroundColor="#6200EE" HeightRequest="150">
            <Label Text="Todo App"
                   TextColor="White"
                   FontSize="28"
                   FontAttributes="Bold"
                   HorizontalOptions="Center"
                   VerticalOptions="Center" />
        </Grid>
    </Shell.FlyoutHeader>

    <!-- Flyout Items -->
    <FlyoutItem Title="My Todos" Icon="todos.png">
        <ShellContent ContentTemplate="{DataTemplate views:TodoListPage}" />
    </FlyoutItem>
    
    <FlyoutItem Title="Categories" Icon="categories.png">
        <ShellContent ContentTemplate="{DataTemplate views:CategoriesPage}" />
    </FlyoutItem>
    
    <FlyoutItem Title="Statistics" Icon="stats.png">
        <ShellContent ContentTemplate="{DataTemplate views:StatisticsPage}" />
    </FlyoutItem>
    
    <FlyoutItem Title="Settings" Icon="settings.png">
        <ShellContent ContentTemplate="{DataTemplate views:SettingsPage}" />
    </FlyoutItem>

    <!-- Flyout Footer -->
    <Shell.FlyoutFooter>
        <StackLayout Padding="15" BackgroundColor="#F0F0F0">
            <Label Text="Made with .NET MAUI"
                   TextColor="Gray"
                   HorizontalOptions="Center" />
        </StackLayout>
    </Shell.FlyoutFooter>

</Shell>
```

### AppShell.xaml.cs

```csharp
namespace TodoApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("tododetails", typeof(TodoDetailsPage));
        Routing.RegisterRoute("addtodo", typeof(AddTodoPage));
        Routing.RegisterRoute("edittodo", typeof(EditTodoPage));
    }
}
```

### Navigate to Details

```csharp
// In TodoListPage.xaml.cs
private async Task OnTodoSelected(TodoItem todo)
{
    await Shell.Current.GoToAsync($"tododetails?todoId={todo.Id}");
}
```

### Receive Data in Details Page

```csharp
namespace TodoApp.Views;

[QueryProperty(nameof(TodoId), "todoId")]
public partial class TodoDetailsPage : ContentPage
{
    private int _todoId;
    public int TodoId
    {
        get => _todoId;
        set
        {
            _todoId = value;
            LoadTodo(value);
        }
    }

    public TodoDetailsPage()
    {
        InitializeComponent();
    }

    private void LoadTodo(int id)
    {
        // Load todo from database
        var todo = App.Database.GetTodo(id);
        
        // Update UI
        TitleLabel.Text = todo.Title;
        DescriptionLabel.Text = todo.Description;
        IsCompletedCheckBox.IsChecked = todo.IsCompleted;
    }

    private async Task OnEditClicked()
    {
        await Shell.Current.GoToAsync($"edittodo?todoId={_todoId}");
    }

    private async Task OnDeleteClicked()
    {
        bool confirm = await DisplayAlert("Confirm",
                                         "Delete this todo?",
                                         "Yes", "No");
        if (confirm)
        {
            App.Database.DeleteTodo(_todoId);
            await Shell.Current.GoToAsync("..");
        }
    }
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Product Catalog Navigation

Create a product catalog with navigation:

**Pages:**
1. ProductListPage - Shows all products
2. ProductDetailsPage - Shows product details
3. CartPage - Shows shopping cart
4. CheckoutPage - Checkout form

**Requirements:**
- Use Shell navigation with Flyout
- Flyout items: Products, Cart, Settings
- Navigate from product list to details
- Pass product ID to details page
- Add "Add to Cart" button in details
- Navigate to cart from flyout
- Show cart item count in flyout badge

### Exercise 2: Multi-Step Form

Create a multi-step registration form:

**Pages:**
1. PersonalInfoPage - Name, email
2. AddressPage - Street, city, country
3. PreferencesPage - Notifications, theme
4. SummaryPage - Review all data

**Requirements:**
- Use modal navigation between steps
- Pass data between pages
- Back button to previous step
- Submit on summary page
- Show confirmation alert

### Exercise 3: Tabbed Notes App

Create a notes app with tabs:

**Tabs:**
1. All Notes - Shows all notes
2. Active - Shows incomplete notes
3. Completed - Shows completed notes

**Requirements:**
- Use TabBar navigation
- Each tab shows filtered notes
- Add note button on each tab
- Navigate to note details
- Edit and delete notes
- Persist notes (use static list for now)

---

## ❓ Common Beginner Questions

### Q: When should I use Shell vs. Modal navigation?

**A**: 
- **Shell**: Main app navigation, structured flow
- **Modal**: Temporary screens, forms, dialogs

### Q: How do I disable the back button?

**A**: Set `NavigationPage.HasBackButton="False"` in XAML or `NavigationPage.SetHasBackButton(this, false)` in C#.

### Q: Can I have multiple Shell instances?

**A**: No, only one Shell instance per app. Use FlyoutItems and Tabs for structure.

### Q: How do I pass data back from a page?

**A**: Use events, messaging, or store in a shared service (dependency injection).

### Q: Why is my navigation not working?

**A**: Check:
1. Route is registered in AppShell
2. Route name matches exactly
3. Page type is correct
4. Using correct navigation method (GoToAsync for Shell)

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **Shell** | Modern navigation container |
| **Flyout** | Side menu (hamburger) |
| **TabBar** | Bottom/top tabs |
| **GoToAsync** | Navigate to route |
| **QueryProperty** | Receive navigation parameters |
| **Modal** | Overlay navigation |
| **OnAppearing** | Page lifecycle event |

---

## ✅ Checklist

Before moving to Lesson 6, make sure you can:

- [ ] Set up AppShell with Flyout or TabBar
- [ ] Register routes for navigation
- [ ] Navigate between pages using GoToAsync
- [ ] Pass data between pages
- [ ] Handle page lifecycle events
- [ ] Use modal navigation
- [ ] Create a multi-page navigation system

---

## 🚀 Next Steps

Excellent! You now understand navigation in MAUI. In the next lesson, we'll learn about **Dependency Injection**, which helps manage services and dependencies in your app.

**Next Lesson**: [Lesson 6: Dependency Injection](06_Dependency_Injection.md)

---

## 📖 Additional Reading

- [Shell Navigation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/)
- [Navigation Stack](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation/)
- [Pass Data Between Pages](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/pass-data)
- [Page Lifecycle](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/page-lifecycle)
