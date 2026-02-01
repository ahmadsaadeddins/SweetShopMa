# MVVM Basics 05: Data Binding Deep Dive

## 🎯 What You'll Learn

By the end of this lesson, you will:
- Understand what data binding is
- Learn the different binding modes
- Master binding syntax
- Handle binding errors
- Use value converters
- Bind to collections

---

## 📖 What is Data Binding?

**Data binding** is the magic that automatically connects your View (UI) to your ViewModel (logic). When data changes in the ViewModel, the View updates automatically. When the user changes data in the View, the ViewModel updates automatically.

### Think of Data Binding as:
- 🔗 **A two-way street** between View and ViewModel
- 📡 **Automatic synchronization** of data
- 🤖 **No manual code needed** to update UI

### Why Use Data Binding?

✅ **Less Code**: No need to manually update UI
✅ **Cleaner Code**: UI logic stays in XAML
✅ **Automatic Updates**: UI updates when data changes
✅ **Testable**: Can test ViewModels without UI
✅ **MVVM Friendly**: Essential for MVVM pattern

---

## 🔄 How Data Binding Works

```
┌──────────────┐                    ┌──────────────┐
│     VIEW     │                    │  VIEWMODEL   │
│              │  Data Binding       │              │
│  <Label      │ ←────────────────→ │  Count       │
│    Text=     │    Two-way sync    │    (int)     │
│    "{Binding │                    │              │
│     Count}"/> │                    │              │
└──────────────┘                    └──────────────┘
```

### The Flow:

1. **ViewModel property changes** → View updates automatically
2. **User types in Entry** → ViewModel property updates automatically
3. **User clicks Button** → ViewModel command executes

---

## 📊 Binding Modes

Binding modes control **how data flows** between View and ViewModel.

### The Four Modes

| Mode | Direction | When to Use | Example |
|------|-----------|-------------|---------|
| **OneWay** | ViewModel → View | Displaying data | Label showing name |
| **TwoWay** | ViewModel ↔ View | User input | Entry for editing |
| **OneWayToSource** | View → ViewModel | Rare cases | Special controls |
| **Default** | Automatic | Let MAUI decide | Most common |

### Examples

```xml
<!-- OneWay: Only display data (ViewModel → View) -->
<Label Text="{Binding UserName}" />
<Label Text="{Binding Email}" />
<Label Text="{Binding Age}" />

<!-- TwoWay: User can edit (ViewModel ↔ View) -->
<Entry Text="{Binding UserName, Mode=TwoWay}" />
<Editor Text="{Binding Description, Mode=TwoWay}" />
<CheckBox IsChecked="{Binding IsActive, Mode=TwoWay}" />
<Switch IsToggled="{Binding NotificationsEnabled, Mode=TwoWay}" />
<Entry Text="{Binding Password, Mode=TwoWay}" IsPassword="True" />

<!-- OneWayToSource: Only send to ViewModel (View → ViewModel) -->
<CheckBox IsChecked="{Binding HasAcceptedTerms, Mode=OneWayToSource}" />

<!-- Default: Let MAUI decide based on control -->
<Picker ItemsSource="{Binding Countries}" SelectedItem="{Binding SelectedCountry}" />
<Slider Value="{Binding Volume}" />
<DatePicker Date="{Binding BirthDate}" />
```

---

## 🎯 Binding Syntax

### Basic Binding

```xml
<!-- Simple binding to a property -->
<Label Text="{Binding UserName}" />
```

### Binding with Mode

```xml
<!-- Specify the binding mode -->
<Entry Text="{Binding Email, Mode=TwoWay}" />
```

### Binding with String Format

```xml
<!-- Format the displayed value -->
<Label Text="{Binding Count, StringFormat='Count: {0}'}" />
<Label Text="{Binding Price, StringFormat='{0:C}'}" /> <!-- Currency -->
<Label Text="{Binding Date, StringFormat='{0:yyyy-MM-dd}'}" /> <!-- Date format -->
```

### Binding with Fallback Value

```xml
<!-- Show this if binding fails -->
<Label Text="{Binding UserName, FallbackValue='Guest'}" />
```

### Binding with Target Null Value

```xml
<!-- Show this if bound value is null -->
<Label Text="{Binding MiddleName, TargetNullValue='(none)'}" />
```

---

## 🔗 BindingContext

The `BindingContext` is the **source** of your bindings. It tells the View which ViewModel to bind to.

### Setting BindingContext in XAML

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.MainPage"
             x:DataType="viewmodels:MainPageViewModel">
    
    <!-- Set BindingContext to the ViewModel -->
    <ContentPage.BindingContext>
        <viewmodels:MainPageViewModel />
    </ContentPage.BindingContext>
    
    <!-- Now all bindings use MainPageViewModel -->
    <Label Text="{Binding UserName}" />
    <Button Text="Save" Command="{Binding SaveCommand}" />
    
</ContentPage>
```

### Setting BindingContext in Code-Behind

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
        // Set BindingContext to ViewModel
        BindingContext = new MainPageViewModel();
    }
}
```

### Setting BindingContext for Specific Controls

```xml
<StackLayout>
    <!-- This StackLayout has its own BindingContext -->
    <StackLayout.BindingContext>
        <viewmodels:UserViewModel />
    </StackLayout.BindingContext>
    
    <Label Text="{Binding Name}" />
    <Entry Text="{Binding Email, Mode=TwoWay}" />
</StackLayout>
```

---

## 🎨 x:DataType for Compile-Time Binding

Using `x:DataType` enables **compiled bindings** which are faster and provide compile-time error checking.

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.MainPage"
             x:DataType="viewmodels:MainPageViewModel">
    
    <!-- Now bindings are compiled and checked at compile-time -->
    <Label Text="{Binding UserName}" />
    
</ContentPage>
```

### Benefits of x:DataType:
✅ **Better Performance**: Compiled bindings are faster
✅ **Compile-Time Errors**: Catch typos early
✅ **IntelliSense**: Better autocomplete in Visual Studio

---

## 🔄 Binding to Collections

### ObservableCollection

Use `ObservableCollection<T>` for collections that notify the View when items are added or removed.

```csharp
// In ViewModel
public ObservableCollection<TodoItem> Todos { get; } = new();
```

```xml
<!-- In View -->
<CollectionView ItemsSource="{Binding Todos}">
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:TodoItem">
            <StackLayout>
                <Label Text="{Binding Title}" />
                <CheckBox IsChecked="{Binding IsCompleted}" />
            </StackLayout>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### ListView Example

```xml
<ListView ItemsSource="{Binding Users}"
          HasUnevenRows="True"
          SelectionMode="None">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="models:User">
            <ViewCell>
                <StackLayout Padding="10" Orientation="Horizontal">
                    <Label Text="{Binding Name}"
                           FontSize="18"
                           VerticalOptions="Center" />
                    <Label Text="{Binding Email}"
                           FontSize="14"
                           TextColor="Gray"
                           VerticalOptions="Center" />
                </StackLayout>
            </ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

---

## 🔄 Value Converters

Value converters transform data between the ViewModel and View.

### Creating a Converter

```csharp
// Converters/BooleanToColorConverter.cs
using System.Globalization;
using Microsoft.Maui.Graphics;

public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
            return Colors.Green;
        return Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

### Using a Converter

```xml
<ContentPage.Resources>
    <ResourceDictionary>
        <converters:BooleanToColorConverter x:Key="BoolToColor" />
    </ResourceDictionary>
</ContentPage.Resources>

<Label Text="Status"
       TextColor="{Binding IsOnline, Converter={StaticResource BoolToColor}}" />
```

### Built-in Converters

MAUI has some built-in converters:

```xml
<!-- Convert to string -->
<Label Text="{Binding Count, Converter={StaticResource IntToStringConverter}}" />

<!-- Convert null to visibility -->
<ActivityIndicator IsRunning="{Binding IsBusy}" />
<Grid IsVisible="{Binding IsBusy}">
    <Label Text="Loading..." />
</Grid>
```

---

## 🎯 Binding with Commands

Commands handle user actions.

```xml
<!-- Simple command -->
<Button Text="Save"
        Command="{Binding SaveCommand}" />

<!-- Command with parameter -->
<Button Text="Delete"
        Command="{Binding DeleteCommand}"
        CommandParameter="{Binding .}" />

<!-- Command with CanExecute (button auto enables/disables) -->
<Button Text="Submit"
        Command="{Binding SubmitCommand}" />
```

---

## 🔍 Debugging Bindings

### Common Binding Issues

1. **Property Name Typo**
   ```xml
   <!-- Wrong: Typo in property name -->
   <Label Text="{Binding UsreName}" />  <!-- Should be UserName -->
   
   <!-- Right: Correct property name -->
   <Label Text="{Binding UserName}" />
   ```

2. **Missing BindingContext**
   ```csharp
   // ViewModel not set
   BindingContext = new MainPageViewModel();  // Don't forget this!
   ```

3. **Wrong Binding Mode**
   ```xml
   <!-- Wrong: Label doesn't need TwoWay -->
   <Label Text="{Binding Count, Mode=TwoWay}" />
   
   <!-- Right: Label only needs OneWay (or Default) -->
   <Label Text="{Binding Count}" />
   ```

4. **Property Not Observable**
   ```csharp
   // Wrong: Regular property won't notify
   public string Name { get; set; }
   
   // Right: Observable property
   [ObservableProperty]
   private string _name;
   ```

### Debugging Tips

```xml
<!-- Enable binding diagnostics -->
<Label Text="{Binding UserName}">
    <Label.Triggers>
        <DataTrigger TargetType="Label"
                     Binding="{Binding UserName}"
                     Value="{x:Null}">
            <Setter Property="Text" Value="Binding Failed!" />
        </DataTrigger>
    </Label.Triggers>
</Label>
```

---

## ✅ Quick Check

**Question**: What is data binding?

<details>
<summary>Answer</summary>

Data binding is the **automatic connection** between View properties and ViewModel properties that keeps them synchronized.

</details>

---

**Question**: What binding mode should you use for an Entry field?

<details>
<summary>Answer</summary>

**TwoWay** mode, because you need to display the value and also allow the user to edit it.

</details>

---

**Question**: What does `x:DataType` do?

<details>
<summary>Answer</summary>

It enables **compiled bindings** which are faster and provide compile-time error checking.

</details>

---

**Question**: What collection type should you use for binding lists?

<details>
<summary>Answer</summary>

**ObservableCollection<T>**, because it notifies the View when items are added or removed.

</details>

---

## 🎓 Key Takeaways

1. **Data Binding = Automatic Sync**: Keeps View and ViewModel synchronized
2. **Binding Modes Control Flow**: Choose OneWay, TwoWay, etc. based on needs
3. **BindingContext is the Source**: Tells View which ViewModel to use
4. **x:DataType Improves Performance**: Use compiled bindings when possible
5. **ObservableCollection for Lists**: Use it for collections that change

---

## 📚 Next Steps

Now that you understand data binding, let's build a complete MVVM app:

- **Next**: [MVVM Basics 06: Building Your First MVVM App](MVVM_Basics_06_First_App.md)
- **Then**: [MVVM Basics 07: Common MVVM Patterns](MVVM_Basics_07_Patterns.md)

---

## 💡 Practice Exercise

**Try This**: Create bindings for a user profile page

**Requirements**:
- Display user's name (Label)
- Display user's email (Label)
- Edit user's bio (Editor, TwoWay)
- Toggle notifications (Switch, TwoWay)
- Save button (Command)

<details>
<summary>See Solution</summary>

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.ProfilePage"
             x:DataType="viewmodels:ProfileViewModel">
    
    <ContentPage.BindingContext>
        <viewmodels:ProfileViewModel />
    </ContentPage.BindingContext>
    
    <StackLayout Padding="20" Spacing="15">
        
        <!-- Display: OneWay (or Default) -->
        <Label Text="{Binding Name}"
               FontSize="24"
               FontAttributes="Bold" />
        
        <Label Text="{Binding Email}"
               FontSize="16"
               TextColor="Gray" />
        
        <!-- Edit: TwoWay -->
        <Label Text="Bio"
               FontAttributes="Bold" />
        <Editor Text="{Binding Bio, Mode=TwoWay}"
                HeightRequest="100"
                Placeholder="Tell us about yourself..." />
        
        <!-- Toggle: TwoWay -->
        <StackLayout Orientation="Horizontal" Spacing="10">
            <Switch IsToggled="{Binding NotificationsEnabled, Mode=TwoWay}" />
            <Label Text="Enable Notifications"
                   VerticalOptions="Center" />
        </StackLayout>
        
        <!-- Command -->
        <Button Text="Save Profile"
                Command="{Binding SaveCommand}"
                Margin="0,20,0,0" />
        
    </StackLayout>
    
</ContentPage>
```

```csharp
// ProfileViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = "John Doe";

    [ObservableProperty]
    private string _email = "john@example.com";

    [ObservableProperty]
    private string _bio;

    [ObservableProperty]
    private bool _notificationsEnabled = true;

    [RelayCommand]
    private void Save()
    {
        // Save profile logic
    }
}
```

</details>

---

**Remember**: Data binding is the glue that holds MVVM together. Master it, and MVVM becomes much easier!
