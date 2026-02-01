# MVVM Basics 09: Quick Reference & Summary

## 🎯 Purpose

This is your **quick reference guide** for MVVM in .NET MAUI. Use this as a cheat sheet when building MVVM apps.

---

## 📚 MVVM at a Glance

### The Three Components

```
┌─────────────────────────────────────────┐
│              VIEW (XAML)                │
│  • User Interface                       │
│  • Data Bindings                        │
│  • Commands                             │
│  • No business logic!                   │
└──────────────┬──────────────────────────┘
               │ Data Binding
               │
┌──────────────▼──────────────────────────┐
│           VIEWMODEL (C#)                │
│  • Business Logic                       │
│  • Observable Properties                │
│  • Commands                             │
│  • No UI code!                          │
└──────────────┬──────────────────────────┘
               │
               │
┌──────────────▼──────────────────────────┐
│            MODEL (C#)                   │
│  • Data Structures                      │
│  • Business Objects                     │
│  • Validation Logic                     │
│  • No UI or Commands!                   │
└─────────────────────────────────────────┘
```

### Key Responsibilities

| Component | Does | Doesn't |
|-----------|------|----------|
| **Model** | Holds data, validates | UI code, commands |
| **View** | Shows UI, collects input | Business logic, data access |
| **ViewModel** | Connects Model to View | UI code (Colors, Fonts) |

---

## 🚀 Quick Start Template

### 1. Install Package

```bash
Install-Package CommunityToolkit.Mvvm
```

### 2. Create Model

```csharp
// Models/TodoItem.cs
namespace MyApp.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}
```

### 3. Create ViewModel

```csharp
// ViewModels/TodoViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Models;

namespace MyApp.ViewModels;

public partial class TodoViewModel : ObservableObject
{
    public ObservableCollection<TodoItem> Todos { get; } = new();

    [ObservableProperty]
    private string _newTodoTitle;

    [RelayCommand]
    private void AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTodoTitle))
            return;

        Todos.Add(new TodoItem
        {
            Id = Todos.Count + 1,
            Title = NewTodoTitle,
            IsCompleted = false
        });

        NewTodoTitle = string.Empty;
    }

    [RelayCommand]
    private void DeleteTodo(TodoItem todo)
    {
        Todos.Remove(todo);
    }
}
```

### 4. Create View

```xml
<!-- Views/TodoPage.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             xmlns:models="clr-namespace:MyApp.Models"
             x:Class="MyApp.Views.TodoPage"
             x:DataType="viewmodels:TodoViewModel">

    <ContentPage.BindingContext>
        <viewmodels:TodoViewModel />
    </ContentPage.BindingContext>

    <StackLayout Padding="20" Spacing="10">
        <Entry Placeholder="What needs to be done?"
               Text="{Binding NewTodoTitle, Mode=TwoWay}" />
        
        <Button Text="Add"
                Command="{Binding AddTodoCommand}" />
        
        <CollectionView ItemsSource="{Binding Todos}">
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:TodoItem">
                    <Frame Padding="10" Margin="0,5">
                        <StackLayout Orientation="Horizontal">
                            <Label Text="{Binding Title}"
                                   VerticalOptions="Center" />
                            <Button Text="Delete"
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:TodoViewModel}}, Path=DeleteTodoCommand}"
                                    CommandParameter="{Binding .}" />
                        </StackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </StackLayout>

</ContentPage>
```

---

## 🎯 Common Patterns

### Observable Property

```csharp
[ObservableProperty]
private string _name;

// Auto-generates: public string Name { get; set; }
```

### Command

```csharp
[RelayCommand]
private void Save()
{
    // Save logic
}

// Auto-generates: public ICommand SaveCommand { get; }
```

### Async Command

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    IsBusy = true;
    try
    {
        // Load data
    }
    finally
    {
        IsBusy = false;
    }
}
```

### Command with Parameter

```csharp
[RelayCommand]
private void DeleteItem(TodoItem item)
{
    Todos.Remove(item);
}
```

```xml
<Button Command="{Binding DeleteItemCommand}"
        CommandParameter="{Binding .}" />
```

### CanExecute

```csharp
[ObservableProperty]
private string _name;

[RelayCommand(CanExecute = nameof(CanSave))]
private void Save()
{
    // Save logic
}

private bool CanSave() => !string.IsNullOrWhiteSpace(Name);

partial void OnNameChanged(string value)
{
    SaveCommand.NotifyCanExecuteChanged();
}
```

---

## 📊 Data Binding Modes

| Mode | Direction | Use For |
|------|-----------|---------|
| **OneWay** | ViewModel → View | Displaying data |
| **TwoWay** | ViewModel ↔ View | User input |
| **OneWayToSource** | View → ViewModel | Rare cases |
| **Default** | Auto | Let MAUI decide |

### Examples

```xml
<!-- OneWay -->
<Label Text="{Binding UserName}" />

<!-- TwoWay -->
<Entry Text="{Binding UserName, Mode=TwoWay}" />

<!-- Default -->
<Switch IsToggled="{Binding IsActive}" />
```

---

## 🎨 Common XAML Elements

### Layouts

```xml
<!-- StackLayout: Stack items vertically or horizontally -->
<StackLayout Orientation="Vertical" Spacing="10">
    <Label Text="Item 1" />
    <Label Text="Item 2" />
</StackLayout>

<!-- Grid: Rows and columns -->
<Grid RowDefinitions="Auto,*" ColumnDefinitions="*,*">
    <Label Grid.Row="0" Grid.Column="0" Text="Top Left" />
    <Label Grid.Row="0" Grid.Column="1" Text="Top Right" />
</Grid>

<!-- FlexLayout: Flexible wrapping -->
<FlexLayout Direction="Row" Wrap="Wrap">
    <Label Text="Item 1" />
    <Label Text="Item 2" />
</FlexLayout>
```

### Controls

```xml
<!-- Display -->
<Label Text="Hello" FontSize="18" />
<Image Source="photo.png" />
<ActivityIndicator IsRunning="{Binding IsBusy}" />

<!-- Input -->
<Entry Text="{Binding Name, Mode=TwoWay}" />
<Editor Text="{Binding Description, Mode=TwoWay}" HeightRequest="100" />
<CheckBox IsChecked="{Binding IsActive, Mode=TwoWay}" />
<Switch IsToggled="{Binding Enabled, Mode=TwoWay}" />
<Picker ItemsSource="{Binding Items}" SelectedItem="{Binding SelectedItem}" />
<DatePicker Date="{Binding Date, Mode=TwoWay}" />

<!-- Actions -->
<Button Text="Save" Command="{Binding SaveCommand}" />
<SearchBar SearchCommand="{Binding SearchCommand}" />
```

### Collections

```xml
<!-- CollectionView: Virtualized list -->
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:Item">
            <StackLayout Padding="10">
                <Label Text="{Binding Name}" />
            </StackLayout>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>

<!-- ListView: Simple list -->
<ListView ItemsSource="{Binding Items}"
          HasUnevenRows="True"
          SelectionMode="None">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="models:Item">
            <TextCell Text="{Binding Name}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

---

## 🔧 Best Practices Checklist

### ✅ DO

- [ ] Use `ObservableProperty` for all ViewModel properties
- [ ] Use `RelayCommand` for all user actions
- [ ] Set `x:DataType` for compiled bindings
- [ ] Use `ObservableCollection<T>` for lists
- [ ] Handle async operations with try/catch/finally
- [ ] Show loading state with `IsBusy`
- [ ] Validate input before processing
- [ ] Use dependency injection for services
- [ ] Keep ViewModels focused and small
- [ ] Use proper naming conventions

### ❌ DON'T

- [ ] Put business logic in Views
- [ ] Put UI code in ViewModels
- [ ] Use event handlers (use Commands)
- [ ] Hardcode strings (use resources)
- [ ] Forget to handle nulls
- [ ] Block the UI thread
- [ ] Create memory leaks
- [ ] Make ViewModels too large
- [ ] Skip error handling
- [ ] Use unclear names

---

## 🎯 Common Scenarios

### Loading Data

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    if (IsBusy)
        return;

    try
    {
        IsBusy = true;
        var data = await _service.GetDataAsync();
        Items = new ObservableCollection<Item>(data);
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
```

### Form Submission

```csharp
[ObservableProperty]
private string _email;

[ObservableProperty]
private string _password;

[RelayCommand(CanExecute = nameof(CanSubmit))]
private async Task SubmitAsync()
{
    await _service.LoginAsync(Email, Password);
}

private bool CanSubmit()
{
    return !string.IsNullOrWhiteSpace(Email) &&
           !string.IsNullOrWhiteSpace(Password) &&
           Password.Length >= 8;
}

partial void OnEmailChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
partial void OnPasswordChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
```

### List with Selection

```csharp
[ObservableProperty]
private ObservableCollection<Item> _items;

[ObservableProperty]
private Item _selectedItem;

[RelayCommand]
private async Task GoToDetailAsync()
{
    if (SelectedItem == null)
        return;

    await _navigation.NavigateToAsync("detail", SelectedItem);
    SelectedItem = null;
}
```

```xml
<CollectionView ItemsSource="{Binding Items}"
                SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
                SelectionMode="Single">
    <!-- Item template -->
</CollectionView>
```

---

## 🚀 Performance Tips

1. **Use Compiled Bindings**: Always set `x:DataType`
2. **Virtualize Lists**: Use `CollectionView` instead of `StackLayout`
3. **Lazy Load**: Load data only when needed
4. **Avoid Memory Leaks**: Unsubscribe from events
5. **Use DataTemplate Selectors**: For complex list items

---

## 📚 Learning Path

1. **Start Here**: [MVVM Basics 01: Introduction](MVVM_Basics_01_Introduction.md)
2. **Then**: [MVVM Basics 02: Model](MVVM_Basics_02_Model.md)
3. **Then**: [MVVM Basics 03: View](MVVM_Basics_03_View.md)
4. **Then**: [MVVM Basics 04: ViewModel](MVVM_Basics_04_ViewModel.md)
5. **Then**: [MVVM Basics 05: Data Binding](MVVM_Basics_05_Data_Binding.md)
6. **Practice**: [MVVM Basics 06: First App](MVVM_Basics_06_First_App.md)
7. **Advanced**: [MVVM Basics 07: Patterns](MVVM_Basics_07_Patterns.md)
8. **Master**: [MVVM Basics 08: Best Practices](MVVM_Basics_08_Best_Practices.md)

---

## 🎓 Key Concepts Summary

### MVVM = Separation of Concerns

- **Model**: Data and business logic
- **View**: User interface
- **ViewModel**: Connection between Model and View

### Data Binding = Automatic Sync

- ViewModel changes → View updates
- User input → ViewModel updates
- No manual code needed

### Commands = User Actions

- Button clicks → Commands in ViewModel
- No code-behind needed
- Clean separation

---

## 💡 Quick Tips

- **Always** use `x:DataType` for compiled bindings
- **Always** use `ObservableCollection<T>` for lists
- **Always** handle async operations with try/catch
- **Always** show loading state with `IsBusy`
- **Always** validate input before processing
- **Never** put business logic in Views
- **Never** put UI code in ViewModels
- **Never** use event handlers (use Commands)
- **Never** hardcode strings (use resources)
- **Never** forget to handle nulls

---

## 🔗 Helpful Resources

### Official Documentation
- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)

### Community
- [.NET MAUI GitHub](https://github.com/dotnet/maui)
- [Stack Overflow - MAUI Tag](https://stackoverflow.com/questions/tagged/maui)

---

## ✅ You're Ready!

You now have all the knowledge you need to build MVVM apps in .NET MAUI. Remember:

1. **Start Simple**: Don't try to build complex apps immediately
2. **Practice**: Build small apps to reinforce learning
3. **Follow Patterns**: Use the patterns you've learned
4. **Stay Organized**: Keep Model, View, and ViewModel separate
5. **Keep Learning**: MVVM has more advanced topics to explore

---

## 🎯 Next Steps

Now that you understand MVVM basics:

1. **Build More Apps**: Practice with different types of apps
2. **Learn Advanced Topics**: Explore dependency injection, testing, etc.
3. **Join the Community**: Ask questions, share knowledge
4. **Read Real Code**: Look at open-source MAUI apps
5. **Teach Others**: Teaching reinforces your learning

---

**Congratulations!** You've completed the MVVM Basics course. Now go build amazing apps with MVVM! 🚀

---

## 📝 Quick Reference Card

### Model
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### ViewModel
```csharp
public partial class UserViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [RelayCommand] private void Save() { }
}
```

### View
```xml
<ContentPage x:DataType="viewmodels:UserViewModel">
    <Entry Text="{Binding Name, Mode=TwoWay}" />
    <Button Command="{Binding SaveCommand}" />
</ContentPage>
```

---

**Happy Coding!** 💻✨
