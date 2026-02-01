# Lesson 4: Data Binding

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand data binding fundamentals
- Learn different binding modes and when to use them
- Master binding syntax and expressions
- Use value converters to transform data
- Implement collection binding with templates
- Handle binding errors and fallbacks

---

## 📖 What is Data Binding?

**Data Binding** is the automatic synchronization of data between your UI (View) and your data source (ViewModel). Instead of manually updating UI elements, bindings handle it for you.

### Why Use Data Binding?

✅ **Less Code** - No manual UI updates needed  
✅ **Automatic Updates** - UI reflects data changes instantly  
✅ **Clean Architecture** - UI separate from business logic  
✅ **Testable** - Test ViewModels without UI  
✅ **Maintainable** - Centralized data flow  

### How Binding Works

```
ViewModel Property (Source)
    ↓
    Binding
    ↓
UI Element Property (Target)
```

When the source changes, the target updates automatically (depending on binding mode).

---

## 🔗 Binding Syntax

### Basic Binding

```xml
<Label Text="{Binding UserName}" />
```

**Breakdown:**
- `{Binding ...}` - Binding expression
- `UserName` - Property name in ViewModel

### Binding with Mode

```xml
<Entry Text="{Binding UserName, Mode=TwoWay}" />
```

### Binding with StringFormat

```xml
<Label Text="{Binding TotalPrice, StringFormat='Total: {0:C}'}" />
```

Result: `Total: $123.45`

### Binding with FallbackValue

```xml
<Label Text="{Binding UserName, FallbackValue='Guest'}" />
```

Shows "Guest" if UserName is null or binding fails.

### Binding with TargetNullValue

```xml
<Label Text="{Binding UserName, TargetNullValue='Not specified'}" />
```

Shows "Not specified" if UserName is null.

---

## 📐 Binding Modes

Binding modes determine how data flows between source and target.

### 1. OneWay (Default for most read-only properties)

Data flows from ViewModel → View only.

```xml
<Label Text="{Binding UserName}" />
<!-- Same as: -->
<Label Text="{Binding UserName, Mode=OneWay}" />
```

**When to use:** Displaying data that user can't edit.

### 2. TwoWay (Default for Entry, CheckBox, Switch, etc.)

Data flows both ways: ViewModel ↔ View.

```xml
<Entry Text="{Binding UserName, Mode=TwoWay}" />
```

**When to use:** User inputs that need to update ViewModel.

### 3. OneWayToSource

Data flows from View → ViewModel only.

```xml
<CheckBox IsChecked="{Binding IsActive, Mode=OneWayToSource}" />
```

**When to use:** Rare. Only need to send user input to ViewModel.

### 4. Default

Let MAUI decide based on the control.

```xml
<Entry Text="{Binding UserName, Mode=Default}" />
<!-- Same as TwoWay for Entry -->
```

### Binding Mode Reference

| Control | Property | Default Mode |
|---------|----------|--------------|
| Label | Text | OneWay |
| Button | Text | OneWay |
| Entry | Text | TwoWay |
| Editor | Text | TwoWay |
| CheckBox | IsChecked | TwoWay |
| Switch | IsToggled | TwoWay |
| Slider | Value | TwoWay |
| DatePicker | Date | TwoWay |
| TimePicker | Time | TwoWay |

---

## 🎨 String Formatting

Format bound values for display.

### Currency Formatting

```xml
<Label Text="{Binding Price, StringFormat='{0:C}'}" />
<!-- Result: $1,234.56 -->
```

### Custom Number Formatting

```xml
<Label Text="{Binding Quantity, StringFormat='{0:N0}'}" />
<!-- Result: 1,234 (no decimals) -->

<Label Text="{Binding Percentage, StringFormat='{0:P2}'}" />
<!-- Result: 25.00% -->
```

### Date Formatting

```xml
<Label Text="{Binding BirthDate, StringFormat='{0:yyyy-MM-dd}'}" />
<!-- Result: 2024-01-15 -->

<Label Text="{Binding BirthDate, StringFormat='{0:MMMM d, yyyy}'}" />
<!-- Result: January 15, 2024 -->
```

### Custom Text

```xml
<Label Text="{Binding UserName, StringFormat='Hello, {0}!'}" />
<!-- Result: Hello, John! -->

<Label Text="{Binding ItemCount, StringFormat='You have {0} items'}" />
<!-- Result: You have 5 items -->
```

### Multiple Values

```xml
<Label>
    <Label.Text>
        <MultiBinding StringFormat="{0} {1}">
            <Binding Path="FirstName" />
            <Binding Path="LastName" />
        </MultiBinding>
    </Label.Text>
</Label>
<!-- Result: John Doe -->
```

---

## 🔄 Value Converters

Converters transform data between source and target.

### Built-in Converters

MAUI includes some built-in converters:

```xml
<!-- Convert integer to visibility -->
<Label IsVisible="{Binding Count, Converter={StaticResource IntToBoolConverter}}" />
```

### Creating Custom Converters

Implement `IValueConverter`:

```csharp
// Converters/BooleanToColorConverter.cs
using System.Globalization;
using Microsoft.Maui.Graphics;

public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return Colors.Green;
        }
        return Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Optional: Convert back from target to source
        if (value is Color color)
        {
            return color == Colors.Green;
        }
        return false;
    }
}
```

### Register Converter in App.xaml

```xml
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:converters="clr-namespace:MyApp.Converters"
             x:Class="MyApp.App">

    <Application.Resources>
        <ResourceDictionary>
            <converters:BooleanToColorConverter x:Key="BoolToColorConverter" />
        </ResourceDictionary>
    </Application.Resources>

</Application>
```

### Use Converter in Binding

```xml
<Label Text="{Binding IsActive}"
       TextColor="{Binding IsActive, Converter={StaticResource BoolToColorConverter}}" />
```

### Converter with Parameter

```csharp
public class ThresholdColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string thresholdStr)
        {
            int threshold = int.Parse(thresholdStr);
            return intValue >= threshold ? Colors.Green : Colors.Red;
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

```xml
<Label Text="{Binding Score}"
       TextColor="{Binding Score, Converter={StaticResource ThresholdColorConverter}, ConverterParameter=60}" />
```

### Multi-Value Converter

Combine multiple values:

```csharp
public class FullNameConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is string first && values[1] is string last)
        {
            return $"{first} {last}";
        }
        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

```xml
<Label>
    <Label.Text>
        <MultiBinding Converter="{StaticResource FullNameConverter}">
            <Binding Path="FirstName" />
            <Binding Path="LastName" />
        </MultiBinding>
    </Label.Text>
</Label>
```

---

## 📋 Collection Binding

Bind lists of data to UI controls.

### ObservableCollection

Use `ObservableCollection` for lists that notify UI of changes:

```csharp
public class TodoViewModel
{
    public ObservableCollection<TodoItem> Todos { get; } = new();
}
```

### ListView Binding

```xml
<ListView ItemsSource="{Binding Todos}"
          HasUnevenRows="True">
    <ListView.ItemTemplate>
        <DataTemplate>
            <ViewCell>
                <StackLayout Padding="10">
                    <Label Text="{Binding Title}"
                           FontSize="18" />
                    <Label Text="{Binding CreatedDate, StringFormat='{0:yyyy-MM-dd}'}"
                           FontSize="12"
                           TextColor="Gray" />
                </StackLayout>
            </ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### CollectionView (Recommended)

More modern and flexible than ListView:

```xml
<CollectionView ItemsSource="{Binding Todos}"
                EmptyView="No todos yet">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Frame Margin="5" Padding="10">
                <StackLayout>
                    <Label Text="{Binding Title}"
                           FontSize="16" />
                    <CheckBox IsChecked="{Binding IsCompleted}" />
                </StackLayout>
            </Frame>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### Picker Binding

```xml
<Picker Title="Select Category"
        ItemsSource="{Binding Categories}"
        SelectedItem="{Binding SelectedCategory}" />
```

### ComboBox Binding (Windows only)

```xml
<ComboBox ItemsSource="{Binding Categories}"
          SelectedItem="{Binding SelectedCategory}"
          PlaceholderText="Select category" />
```

---

## 🎯 Advanced Binding Scenarios

### Binding to Nested Properties

```xml
<Label Text="{Binding User.Address.City}" />
```

### Binding with Indexer

```xml
<Label Text="{Binding Items[0].Name}" />
```

### Binding to Attached Properties

```xml
<Label Grid.Row="{Binding RowIndex}" Text="Hello" />
```

### RelativeSource Binding

Bind to element's own properties or ancestors:

```xml
<!-- Bind to self -->
<Label Text="{Binding Source={RelativeSource Self}, Path=Opacity}" />

<!-- Bind to parent -->
<Button Text="{Binding Source={RelativeSource AncestorType={x:Type StackLayout}}, Path=Children.Count}" />
```

### ElementName Binding

Bind to another named element:

```xml
<Slider x:Name="OpacitySlider" Value="0.5" />
<Label Text="Hello"
       Opacity="{Binding Source={x:Reference OpacitySlider}, Path=Value}" />
```

---

## 🛡️ Binding Validation

Validate user input automatically.

### Implement INotifyDataErrorInfo

```csharp
public class ValidatableObject : ObservableObject
{
    private string _value;
    private string _error;
    private bool _isValid;

    public string Value
    {
        get => _value;
        set
        {
            SetProperty(ref _value, value);
            Validate();
        }
    }

    public string Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            Error = "This field is required";
            IsValid = false;
        }
        else
        {
            Error = string.Empty;
            IsValid = true;
        }
    }
}
```

### Display Validation Errors

```xml
<Entry Text="{Binding Email.Value, Mode=TwoWay}" />
<Label Text="{Binding Email.Error}"
       TextColor="Red"
       IsVisible="{Binding Email.IsValid, Converter={StaticResource InverseBoolConverter}}" />
```

---

## 🧪 Practice Exercises

### Exercise 1: Product List with Formatting

Create a product list with:

**Model:**
```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsAvailable => Stock > 0;
}
```

**Requirements:**
- Display products in CollectionView
- Format price as currency
- Show "In Stock" or "Out of Stock" based on availability
- Use converter to change background color based on stock level
  - Green: Stock > 10
  - Yellow: Stock 1-10
  - Red: Stock = 0

### Exercise 2: User Profile with Multi-Binding

Create a user profile page:

**ViewModel:**
```csharp
public class UserProfileViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}
```

**Requirements:**
- Display full name using multi-binding converter
- Display location as "City, Country" using multi-binding
- Show age with text: "Age: X years"
- Use converter to show "Minor" or "Adult" based on age

### Exercise 3: Search with Filter

Create a searchable list:

**Requirements:**
- Entry for search
- CollectionView displaying items
- Filter items as user types (real-time)
- Show "No results found" when filter matches nothing
- Highlight: Use `ObservableCollection` and filter in ViewModel

**Hint**: Create a filtered collection property that updates when search text changes.

---

## ❓ Common Beginner Questions

### Q: Why is my binding not working?

**A**: Check:
1. BindingContext is set to ViewModel
2. Property name matches exactly (case-sensitive)
3. Property is public (not private)
4. Property has setter if using TwoWay binding
5. ViewModel inherits from ObservableObject
6. Property has [ObservableProperty] attribute

### Q: What's the difference between TargetNullValue and FallbackValue?

**A**: 
- `TargetNullValue` - Used when source value is null
- `FallbackValue` - Used when binding fails entirely (path not found, etc.)

### Q: How do I debug bindings?

**A**: 
1. Use Output window in Visual Studio (set build to Debug)
2. Look for binding errors in output
3. Use `x:DataType` for compile-time checking
4. Test with hardcoded values first

### Q: Can I bind to methods?

**A**: No, bindings work with properties only. Use commands for methods.

### Q: When should I use converter vs. computed property?

**A**: 
- **Converter**: Reusable across multiple bindings
- **Computed property**: Specific to one ViewModel, simpler

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **OneWay** | ViewModel → View |
| **TwoWay** | ViewModel ↔ View |
| **StringFormat** | Format bound values for display |
| **Converter** | Transform data between source and target |
| **ObservableCollection** | Collection that notifies UI of changes |
| **CollectionView** | Modern control for displaying lists |
| **Validation** | Ensure data correctness |

---

## ✅ Checklist

Before moving to Lesson 5, make sure you can:

- [ ] Create basic bindings between View and ViewModel
- [ ] Use different binding modes appropriately
- [ ] Format bound values with StringFormat
- [ ] Create and use value converters
- [ ] Bind collections to CollectionView
- [ ] Handle binding errors with FallbackValue
- [ ] Implement validation in bindings

---

## 🚀 Next Steps

Great job mastering data binding! In the next lesson, we'll learn about **Navigation** and how to move between different pages in your MAUI app.

**Next Lesson**: [Lesson 5: Navigation](05_Navigation.md)

---

## 📖 Additional Reading

- [Data Binding Overview](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/)
- [Binding Modes](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/binding-modes)
- [Value Converters](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/converters)
- [CollectionView Documentation](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/collectionview)
