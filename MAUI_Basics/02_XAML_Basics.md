# Lesson 2: XAML Basics

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand what XAML is and why we use it
- Learn basic XAML syntax and structure
- Master common MAUI controls
- Understand layouts and how to arrange UI elements
- Style your UI with colors, fonts, and spacing
- Handle user interactions with events

---

## 📖 What is XAML?

**XAML** (eXtensible Application Markup Language, pronounced "zamel") is a declarative markup language used to define user interfaces in .NET MAUI.

### Why Use XAML?

✅ **Separation of Concerns** - UI (XAML) separate from logic (C#)  
✅ **Readable & Maintainable** - Easy to understand UI structure  
✅ **Tool Support** - Visual Studio provides IntelliSense and preview  
✅ **Hot Reload** - See UI changes instantly while app runs  
✅ **Designer Friendly** - Designers can work on XAML without C# knowledge

### XAML vs C# for UI

| XAML (Declarative) | C# (Imperative) |
|-------------------|----------------|
| Describes WHAT the UI looks like | Describes HOW to create the UI |
| `<Label Text="Hello" />` | `var label = new Label { Text = "Hello" };` |
| Better for complex layouts | Better for dynamic UI generation |
| Supports Hot Reload | Requires rebuild |

**Best Practice**: Use XAML for static UI, C# for dynamic creation and logic.

---

## 🔤 XAML Syntax Basics

### Element Syntax

Every UI element is represented as an XML element:

```xml
<Label Text="Hello, World!" />
```

### Attribute Syntax

Properties are set as attributes:

```xml
<Label Text="Hello" 
       FontSize="18" 
       TextColor="Blue" />
```

### Property Element Syntax

For complex properties, use property element syntax:

```xml
<Label>
    <Label.Text>
        Hello, World!
    </Label.Text>
</Label>
```

### Content Property

Many controls have a default content property:

```xml
<Button>
    Click Me!
    <!-- Same as: <Button Text="Click Me!" /> -->
</Button>
```

---

## 📦 Common MAUI Controls

### 1. Label - Display Text

```xml
<Label Text="Hello, World!" 
       FontSize="24"
       FontAttributes="Bold"
       TextColor="Blue"
       HorizontalOptions="Center"
       VerticalOptions="Center" />
```

**Key Properties:**
- `Text` - The text to display
- `FontSize` - Size in pixels (default ~14)
- `FontAttributes` - None, Bold, Italic, or BoldItalic
- `TextColor` - Color of text
- `HorizontalOptions` - Start, Center, End, Fill, etc.
- `VerticalOptions` - Start, Center, End, Fill, etc.

### 2. Button - Clickable Action

```xml
<Button Text="Click Me"
        Clicked="OnButtonClicked"
        BackgroundColor="Blue"
        TextColor="White"
        CornerRadius="8"
        WidthRequest="150"
        HeightRequest="50" />
```

**Key Properties:**
- `Text` - Button text
- `Clicked` - Event handler for clicks
- `Command` - Alternative to Clicked (used with MVVM)
- `BackgroundColor` - Button background color
- `TextColor` - Text color
- `CornerRadius` - Rounded corners (0 = square)
- `IsEnabled` - Enable/disable button
- `WidthRequest` / `HeightRequest` - Size

### 3. Entry - Single-line Text Input

```xml
<Entry Placeholder="Enter your name"
       Text="{Binding UserName}"
       Keyboard="Text"
       MaxLength="50"
       IsPassword="False" />
```

**Key Properties:**
- `Placeholder` - Hint text shown when empty
- `Text` - Current text value
- `Keyboard` - Text, Email, Phone, Numeric, Url
- `MaxLength` - Maximum characters allowed
- `IsPassword` - Hide text (for passwords)
- `IsReadOnly` - Prevent editing

### 4. Editor - Multi-line Text Input

```xml
<Editor Placeholder="Enter your message"
        Text="{Binding Message}"
        AutoSize="TextChanges"
        MaxLength="500" />
```

**Key Properties:**
- Similar to Entry, but supports multiple lines
- `AutoSize` - TextChanges, or Disabled

### 5. Image - Display Pictures

```xml
<Image Source="myimage.png"
       WidthRequest="200"
       HeightRequest="200"
       Aspect="AspectFit" />
```

**Key Properties:**
- `Source` - Image file name or URL
- `Aspect` - AspectFit, AspectFill, Fill
- `WidthRequest` / `HeightRequest` - Size
- `Opacity` - Transparency (0.0 to 1.0)

### 6. CheckBox - Boolean Selection

```xml
<CheckBox IsChecked="True"
          CheckedChanged="OnCheckBoxChanged" />
```

### 7. Slider - Numeric Range Selection

```xml
<Slider Minimum="0"
        Maximum="100"
        Value="50"
        ValueChanged="OnSliderValueChanged" />
```

### 8. Switch - Toggle On/Off

```xml
<Switch IsToggled="True"
        Toggled="OnSwitchToggled" />
```

### 9. DatePicker - Date Selection

```xml
<DatePicker Date="{Binding BirthDate}"
            MinimumDate="1900-01-01"
            MaximumDate="2100-12-31" />
```

### 10. TimePicker - Time Selection

```xml
<TimePicker Time="{Binding AlarmTime}" />
```

---

## 📐 Layouts - Arranging UI Elements

Layouts are containers that arrange their children. Every MAUI page has a layout at its root.

### 1. VerticalStackLayout - Stack Vertically

Arranges children in a vertical column:

```xml
<VerticalStackLayout Spacing="10" Padding="20">
    <Label Text="First" />
    <Label Text="Second" />
    <Label Text="Third" />
</VerticalStackLayout>
```

**Result:**
```
First
Second
Third
```

**Key Properties:**
- `Spacing` - Space between children (default 0)
- `Padding` - Space around the layout (left, top, right, bottom)

### 2. HorizontalStackLayout - Stack Horizontally

Arranges children in a horizontal row:

```xml
<HorizontalStackLayout Spacing="10">
    <Button Text="1" />
    <Button Text="2" />
    <Button Text="3" />
</HorizontalStackLayout>
```

**Result:**
```
[1] [2] [3]
```

### 3. Grid - Rows and Columns

Most flexible layout with rows and columns:

```xml
<Grid RowDefinitions="Auto, *, Auto"
      ColumnDefinitions="*, *"
      RowSpacing="10"
      ColumnSpacing="10">
    
    <!-- Row 0, Column 0 (spans 2 columns) -->
    <Label Text="Header"
           Grid.Row="0"
           Grid.ColumnSpan="2" />
    
    <!-- Row 1, Column 0 -->
    <Label Text="Left"
           Grid.Row="1"
           Grid.Column="0" />
    
    <!-- Row 1, Column 1 -->
    <Label Text="Right"
           Grid.Row="1"
           Grid.Column="1" />
    
    <!-- Row 2, Column 0 (spans 2 columns) -->
    <Button Text="Footer"
            Grid.Row="2"
            Grid.ColumnSpan="2" />
    
</Grid>
```

**Row/Column Definitions:**
- `Auto` - Size to fit content
- `*` - Take remaining space (proportional)
- `100` - Fixed size in pixels

**Examples:**
- `"Auto, *, Auto"` - Header (auto), content (fills), footer (auto)
- `"*, 2*"` - First column gets 1/3, second gets 2/3
- `"200, *"` - First column fixed 200px, second fills rest

**Grid Positioning:**
- `Grid.Row` - Row index (0-based)
- `Grid.Column` - Column index (0-based)
- `Grid.RowSpan` - Number of rows to span
- `Grid.ColumnSpan` - Number of columns to span

### 4. FlexLayout - Flexible Wrapping

Advanced layout that can wrap items:

```xml
<FlexLayout Direction="Row"
            Wrap="Wrap"
            JustifyContent="SpaceAround"
            AlignItems="Center">
    
    <Button Text="1" WidthRequest="80" />
    <Button Text="2" WidthRequest="80" />
    <Button Text="3" WidthRequest="80" />
    <Button Text="4" WidthRequest="80" />
    
</FlexLayout>
```

### 5. AbsoluteLayout - Precise Positioning

Position children at exact coordinates:

```xml
<AbsoluteLayout>
    <BoxView Color="Red"
             AbsoluteLayout.LayoutBounds="0, 0, 100, 100" />
    
    <BoxView Color="Blue"
             AbsoluteLayout.LayoutBounds="50, 50, 100, 100" />
</AbsoluteLayout>
```

**LayoutBounds format:** `x, y, width, height`

### 6. ScrollView - Scrollable Content

Allows content to scroll when it's too large:

```xml
<ScrollView>
    <VerticalStackLayout>
        <!-- Many items here -->
        <Label Text="Item 1" />
        <Label Text="Item 2" />
        <!-- ... 100 more items ... -->
    </VerticalStackLayout>
</ScrollView>
```

**Orientation:**
- `Vertical` (default) - Scroll vertically
- `Horizontal` - Scroll horizontally
- `Both` - Scroll in both directions

---

## 🎨 Styling and Appearance

### Colors

MAUI supports multiple color formats:

```xml
<!-- Named color -->
<Button Text="Button" BackgroundColor="Blue" />

<!-- Hex color -->
<Button Text="Button" BackgroundColor="#FF0000" />

<!-- RGB -->
<Button Text="Button" BackgroundColor="rgb(255, 0, 0)" />

<!-- RGBA (with alpha/transparency) -->
<Button Text="Button" BackgroundColor="rgba(255, 0, 0, 0.5)" />

<!-- HSL -->
<Button Text="Button" BackgroundColor="hsl(0, 100%, 50%" />
```

### Fonts

```xml
<!-- System font -->
<Label Text="Hello" FontSize="18" />

<!-- Custom font (must be registered in MauiProgram.cs) -->
<Label Text="Hello" 
       FontFamily="OpenSansSemibold" 
       FontSize="24" />
```

### Spacing

```xml
<VerticalStackLayout Padding="20, 10, 20, 10">
    <!-- Padding: left, top, right, bottom -->
    
    <Label Text="Item 1" Margin="10" />
    <!-- Margin: 10 on all sides -->
    
    <Label Text="Item 2" Margin="0, 10, 0, 10" />
    <!-- Margin: left, top, right, bottom -->
</VerticalStackLayout>
```

**Difference:**
- `Padding` - Space INSIDE a control's boundary
- `Margin` - Space OUTSIDE a control's boundary

### Alignment

```xml
<Label Text="Centered"
       HorizontalOptions="Center"
       VerticalOptions="Center" />
```

**Options:**
- `Start` - Align to start (left for LTR, right for RTL)
- `Center` - Center in available space
- `End` - Align to end (right for LTR, left for RTL)
- `Fill` - Fill available space
- `StartAndExpand` - Start and expand to fill
- `CenterAndExpand` - Center and expand to fill
- `EndAndExpand` - End and expand to fill
- `FillAndExpand` - Fill and expand to fill

---

## 🔄 Handling Events

Events are actions that occur when user interacts with controls.

### Clicked Event (Button)

**XAML:**
```xml
<Button Text="Click Me" Clicked="OnButtonClicked" />
```

**C#:**
```csharp
private void OnButtonClicked(object sender, EventArgs e)
{
    // This runs when button is clicked
    DisplayAlert("Alert", "Button was clicked!", "OK");
}
```

### TextChanged Event (Entry)

**XAML:**
```xml
<Entry TextChanged="OnEntryTextChanged" />
```

**C#:**
```csharp
private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
{
    string oldText = e.OldTextValue;
    string newText = e.NewTextValue;
    
    Console.WriteLine($"Text changed from '{oldText}' to '{newText}'");
}
```

### Completed Event (Entry)

Fires when user presses "Enter" or "Done":

**XAML:**
```xml
<Entry Placeholder="Type and press Enter"
       Completed="OnEntryCompleted" />
```

**C#:**
```csharp
private void OnEntryCompleted(object sender, EventArgs e)
{
    var entry = (Entry)sender;
    string text = entry.Text;
    
    DisplayAlert("You typed", text, "OK");
}
```

### ValueChanged Event (Slider)

**XAML:**
```xml
<Slider ValueChanged="OnSliderValueChanged" />
<Label x:Name="ValueLabel" Text="0" />
```

**C#:**
```csharp
private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
{
    double newValue = e.NewValue;
    ValueLabel.Text = newValue.ToString("F0"); // Format as integer
}
```

---

## 🎯 Complete Example: Login Form

Let's put it all together with a practical example:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyFirstMauiApp.LoginPage"
             Title="Login">
    
    <Grid RowDefinitions="Auto, *, Auto"
          Padding="20">
        
        <!-- Header -->
        <Label Text="Welcome Back!"
               FontSize="32"
               FontAttributes="Bold"
               HorizontalOptions="Center"
               Grid.Row="0" />
        
        <!-- Form -->
        <VerticalStackLayout Spacing="15"
                             Grid.Row="1"
                             VerticalOptions="Center">
            
            <Label Text="Username"
                   FontSize="16"
                   FontAttributes="Bold" />
            
            <Entry x:Name="UsernameEntry"
                   Placeholder="Enter your username"
                   AutoCorrect="False" />
            
            <Label Text="Password"
                   FontSize="16"
                   FontAttributes="Bold" />
            
            <Entry x:Name="PasswordEntry"
                   Placeholder="Enter your password"
                   IsPassword="True" />
            
            <CheckBox x:Name="RememberMeCheckBox"
                      Text="Remember me" />
            
            <Button Text="Login"
                    Clicked="OnLoginClicked"
                    BackgroundColor="#007AFF"
                    TextColor="White"
                    CornerRadius="8"
                    HeightRequest="50" />
            
        </VerticalStackLayout>
        
        <!-- Footer -->
        <Label Text="Don't have an account? Sign up"
               HorizontalOptions="Center"
               Grid.Row="2" />
        
    </Grid>

</ContentPage>
```

**Code-Behind:**
```csharp
public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;
        bool rememberMe = RememberMeCheckBox.IsChecked;

        // Validate
        if (string.IsNullOrWhiteSpace(username) || 
            string.IsNullOrWhiteSpace(password))
        {
            DisplayAlert("Error", 
                        "Please enter username and password", 
                        "OK");
            return;
        }

        // TODO: Implement actual login logic
        DisplayAlert("Success", 
                    $"Welcome, {username}!\nRemember me: {rememberMe}", 
                    "OK");
    }
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Registration Form

Create a registration form with:
- Name (Entry)
- Email (Entry with Email keyboard)
- Password (Entry with IsPassword)
- Confirm Password (Entry with IsPassword)
- Date of Birth (DatePicker)
- Terms Checkbox
- Register Button

**Requirements:**
- Validate passwords match
- Validate email contains "@"
- Show alert with all data on success

### Exercise 2: BMI Calculator

Create a BMI calculator with:
- Height Entry (in cm)
- Weight Entry (in kg)
- Calculate Button
- Result Label
- Color-coded result:
  - Underweight (< 18.5): Blue
  - Normal (18.5-24.9): Green
  - Overweight (25-29.9): Yellow
  - Obese (≥ 30): Red

**Formula**: `BMI = weight (kg) / height (m)²`

### Exercise 3: Todo List UI

Create a todo list UI with:
- Entry for new todo
- "Add" Button
- VerticalStackLayout to display todos
- Each todo should have:
  - Label with todo text
  - Delete Button

**Hint**: You'll need to dynamically add controls in C# code.

---

## ❓ Common Beginner Questions

### Q: What's the difference between WidthRequest and Width?

**A**: 
- `WidthRequest` - Requested width (may be ignored by layout)
- `Width` - Actual rendered width (read-only, calculated by layout)

### Q: Why doesn't my Button click handler work?

**A**: Make sure:
1. The event handler name matches exactly (case-sensitive)
2. The method is `private void` with `(object sender, EventArgs e)` parameters
3. The method is in the code-behind file

### Q: How do I center something on the screen?

**A**: Use `HorizontalOptions="Center"` and `VerticalOptions="Center"`, and ensure the parent layout has space.

### Q: What's the difference between Margin and Padding?

**A**: 
- `Margin` - Space OUTSIDE the control (pushes other things away)
- `Padding` - Space INSIDE the control (pushes content away from edges)

### Q: Why is my layout not working as expected?

**A**: Common issues:
1. Forgot to set `RowDefinitions`/`ColumnDefinitions` on Grid
2. Control is not in the correct row/column
3. Layout doesn't have enough space (check parent layout)
4. Using `WidthRequest`/`HeightRequest` when you want `Fill`

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **XAML** | Declarative markup for UI |
| **Controls** | UI elements like Button, Label, Entry |
| **Layouts** | Containers that arrange controls |
| **Events** | Actions triggered by user interaction |
| **Properties** | Control attributes (Text, Color, etc.) |
| **Hot Reload** | See XAML changes instantly |

---

## ✅ Checklist

Before moving to Lesson 3, make sure you can:

- [ ] Create basic UI controls (Label, Button, Entry)
- [ ] Use layouts (VerticalStackLayout, HorizontalStackLayout, Grid)
- [ ] Handle button clicks and other events
- [ ] Style controls with colors, fonts, and spacing
- [ ] Align controls using HorizontalOptions/VerticalOptions
- [ ] Build a simple form with validation

---

## 🚀 Next Steps

Excellent! You now understand XAML basics. In the next lesson, we'll learn the **MVVM Pattern**, which is the professional way to build MAUI apps by separating UI from business logic.

**Next Lesson**: [Lesson 3: MVVM Pattern](03_MVVM_Pattern.md)

---

## 📖 Additional Reading

- [XAML Syntax Guide](https://learn.microsoft.com/en-us/dotnet/maui/xaml/)
- [Controls Reference](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/)
- [Layouts Guide](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/layouts/)
- [XAML Hot Reload](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/hot-reload)
