# Effects in .NET MAUI - A Beginner's Guide

## Table of Contents
1. [What are Effects?](#what-are-effects)
2. [Effects vs Custom Renderers](#effects-vs-custom-renderers)
3. [When to Use Effects](#when-to-use-effects)
4. [Your First Effect](#your-first-effect)
5. [Platform-Specific Implementation](#platform-specific-implementation)
6. [Reusable Effects](#reusable-effects)
7. [Common Use Cases](#common-use-cases)
8. [Best Practices](#best-practices)

---

## What are Effects?

**Effects** allow you to customize the appearance and behavior of .NET MAUI controls without creating full custom renderers. They're a lightweight way to add platform-specific styling and functionality.

### Simple Analogy
Think of **Custom Renderers** as **building a custom car** from scratch, while **Effects** are like **adding accessories** (spoiler, tinted windows, custom rims) to an existing car.

### Key Characteristics
- 🎯 **Lightweight** - Less code than custom renderers
- 🔧 **Non-intrusive** - Don't replace the control, just modify it
- ♻️ **Reusable** - Can be applied to multiple controls
- 🚀 **Simple** - Easier to implement and maintain

---

## Effects vs Custom Renderers

| Feature | Effects | Custom Renderers |
|---------|---------|------------------|
| **Complexity** | Simple | Complex |
| **Code Required** | Minimal | Extensive |
| **Performance** | Better | Good |
| **Use Case** | Small tweaks | Complete overhauls |
| **Control Replacement** | No | Yes |
| **Learning Curve** | Easy | Steep |

### Decision Tree
```
Need to customize control?
    │
    ├─→ Just change appearance? ──→ Use Effects
    │
    └─→ Need to replace control or add major functionality? ──→ Use Custom Renderers
```

---

## When to Use Effects

✅ **Perfect for Effects:**
- Adding shadows to controls
- Changing fonts or text rendering
- Adding touch feedback effects
- Modifying borders and corners
- Adding platform-specific visual tweaks

❌ **Use Custom Renderers Instead:**
- Completely replacing a control's behavior
- Adding complex gestures
- Implementing custom drawing
- Major performance optimizations

---

## Your First Effect

### Step 1: Create the Effect Class

Create a routing effect in your shared project:

```csharp
// Effects/ShadowEffect.cs
using Microsoft.Maui.Controls;

namespace SweetShopMa.Effects
{
    // This is the routing effect - shared across all platforms
    public class ShadowEffect : RoutingEffect
    {
        // Unique ID for this effect
        public const string EffectName = "SweetShopMa.ShadowEffect";

        public ShadowEffect() 
            : base(EffectName)
        {
        }

        // Properties to configure the shadow
        public Color ShadowColor { get; set; } = Colors.Black;
        public float ShadowRadius { get; set; } = 10f;
        public float ShadowOpacity { get; set; } = 0.5f;
        public float ShadowOffsetX { get; set; } = 0f;
        public float ShadowOffsetY { get; set; } = 2f;
    }
}
```

### Step 2: Create Platform-Specific Effects

#### Android Effect

```csharp
// Platforms/Android/Effects/ShadowEffect.cs
using Android.Graphics;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using SweetShopMa.Effects;

[assembly: ResolutionGroupName("SweetShopMa")]
[assembly: ExportEffect(typeof(ShadowEffect), nameof(ShadowEffect))]
namespace SweetShopMa.Platforms.Android.Effects
{
    public class ShadowEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                if (Control is Android.Views.View view && 
                    Element is Microsoft.Maui.Controls.VisualElement element)
                {
                    // Get the effect from the element
                    var shadowEffect = element.Effects
                        .OfType<SweetShopMa.Effects.ShadowEffect>()
                        .FirstOrDefault();

                    if (shadowEffect != null)
                    {
                        // Apply shadow to the view
                        view.Elevation = 10f;
                        view.TranslationZ = 10f;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to attach shadow effect: {ex.Message}");
            }
        }

        protected override void OnDetached()
        {
            try
            {
                if (Control is Android.Views.View view)
                {
                    // Remove shadow
                    view.Elevation = 0f;
                    view.TranslationZ = 0f;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to detach shadow effect: {ex.Message}");
            }
        }
    }
}
```

#### iOS Effect

```csharp
// Platforms/iOS/Effects/ShadowEffect.cs
using CoreGraphics;
using Foundation;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using SweetShopMa.Effects;

[assembly: ResolutionGroupName("SweetShopMa")]
[assembly: ExportEffect(typeof(ShadowEffect), nameof(ShadowEffect))]
namespace SweetShopMa.Platforms.iOS.Effects
{
    public class ShadowEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                if (Control is UIView view && 
                    Element is Microsoft.Maui.Controls.VisualElement element)
                {
                    // Get the effect from the element
                    var shadowEffect = element.Effects
                        .OfType<SweetShopMa.Effects.ShadowEffect>()
                        .FirstOrDefault();

                    if (shadowEffect != null)
                    {
                        // Apply shadow
                        view.Layer.ShadowColor = UIColor.Black.CGColor;
                        view.Layer.ShadowOffset = new CGSize(0, 2);
                        view.Layer.ShadowOpacity = 0.3f;
                        view.Layer.ShadowRadius = 4f;
                        view.Layer.MasksToBounds = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to attach shadow effect: {ex.Message}");
            }
        }

        protected override void OnDetached()
        {
            try
            {
                if (Control is UIView view)
                {
                    // Remove shadow
                    view.Layer.ShadowOpacity = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to detach shadow effect: {ex.Message}");
            }
        }
    }
}
```

#### Windows Effect

```csharp
// Platforms/Windows/Effects/ShadowEffect.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using SweetShopMa.Effects;

[assembly: ResolutionGroupName("SweetShopMa")]
[assembly: ExportEffect(typeof(ShadowEffect), nameof(ShadowEffect))]
namespace SweetShopMa.Platforms.Windows.Effects
{
    public class ShadowEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                if (Control is FrameworkElement element)
                {
                    // Apply shadow using theme shadow
                    var shadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
                    
                    // Translation is required for shadow to appear
                    element.Translation = new System.Numerics.Vector3(0, 0, 16);
                    element.Shadow = shadow;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to attach shadow effect: {ex.Message}");
            }
        }

        protected override void OnDetached()
        {
            try
            {
                if (Control is FrameworkElement element)
                {
                    element.Translation = new System.Numerics.Vector3(0, 0, 0);
                    element.Shadow = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to detach shadow effect: {ex.Message}");
            }
        }
    }
}
```

### Step 3: Apply the Effect in XAML

```xml
<!-- MainPage.xaml -->
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:effects="clr-namespace:SweetShopMa.Effects"
    x:Class="SweetShopMa.Views.MainPage">

    <StackLayout Padding="20" Spacing="20">
        <!-- Button with Shadow Effect -->
        <Button Text="Shadow Button" 
                BackgroundColor="Purple"
                TextColor="White">
            <Button.Effects>
                <effects:ShadowEffect />
            </Button.Effects>
        </Button>

        <!-- Label with Shadow Effect -->
        <Label Text="Shadow Label" 
               FontSize="24"
               HorizontalOptions="Center">
            <Label.Effects>
                <effects:ShadowEffect />
            </Label.Effects>
        </Label>

        <!-- Frame with Shadow Effect -->
        <Frame Padding="20" BackgroundColor="LightGray">
            <Frame.Effects>
                <effects:ShadowEffect />
            </Frame.Effects>
            <Label Text="Frame with Shadow" />
        </Frame>
    </StackLayout>
</ContentPage>
```

### Step 4: Apply the Effect in C#

```csharp
// MainPage.xaml.cs
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
        // Add shadow effect to a button
        var button = new Button
        {
            Text = "Shadow Button",
            BackgroundColor = Colors.Purple,
            TextColor = Colors.White
        };
        
        button.Effects.Add(new SweetShopMa.Effects.ShadowEffect());
        
        // Add to layout
        Content = new StackLayout
        {
            Padding = 20,
            Children = { button }
        };
    }
}
```

---

## Platform-Specific Implementation

### Effect 1: Focus Effect (Android/iOS)

Create an effect that changes the border color when a control has focus.

**Shared Code:**
```csharp
// Effects/FocusEffect.cs
namespace SweetShopMa.Effects
{
    public class FocusEffect : RoutingEffect
    {
        public const string EffectName = "SweetShopMa.FocusEffect";
        
        public Color FocusColor { get; set; } = Colors.Blue;
        public Color UnfocusedColor { get; set; } = Colors.Gray;
        public float BorderWidth { get; set; } = 2f;
        
        public FocusEffect() 
            : base(EffectName)
        {
        }
    }
}
```

**Android Implementation:**
```csharp
// Platforms/Android/Effects/FocusEffect.cs
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using SweetShopMa.Effects;

[assembly: ResolutionGroupName("SweetShopMa")]
[assembly: ExportEffect(typeof(FocusEffect), nameof(FocusEffect))]
namespace SweetShopMa.Platforms.Android.Effects
{
    public class FocusEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is Android.Views.View view)
            {
                view.FocusChange += OnFocusChange;
            }
        }

        protected override void OnDetached()
        {
            if (Control is Android.Views.View view)
            {
                view.FocusChange -= OnFocusChange;
            }
        }

        void OnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (Control is AndroidX.AppCompat.Widget.AppCompatEditText editText)
                {
                    var focusEffect = Element?.Effects
                        .OfType<SweetShopMa.Effects.FocusEffect>()
                        .FirstOrDefault();

                    if (focusEffect != null)
                    {
                        var gd = new GradientDrawable();
                        gd.SetColor(global::Android.Graphics.Color.White);
                        
                        if (e.HasFocus)
                        {
                            gd.SetStroke(
                                (int)focusEffect.BorderWidth,
                                focusEffect.FocusColor.ToAndroid());
                        }
                        else
                        {
                            gd.SetStroke(
                                (int)focusEffect.BorderWidth,
                                focusEffect.UnfocusedColor.ToAndroid());
                        }
                        
                        editText.Background = gd;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Focus change error: {ex.Message}");
            }
        }
    }
}
```

### Effect 2: Touch Feedback Effect (Android)

Create an effect that adds ripple effect to buttons.

```csharp
// Platforms/Android/Effects/TouchFeedbackEffect.cs
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using SweetShopMa.Effects;

[assembly: ResolutionGroupName("SweetShopMa")]
[assembly: ExportEffect(typeof(TouchFeedbackEffect), nameof(TouchFeedbackEffect))]
namespace SweetShopMa.Platforms.Android.Effects
{
    public class TouchFeedbackEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is Android.Views.View view)
            {
                // Create ripple drawable
                var outValue = new TypedValue();
                view.Context.Theme.ResolveAttribute(
                    Android.Resource.Attribute.SelectableItemBackground, 
                    outValue, 
                    true);

                view.SetBackground(
                    view.Context.GetDrawable(outValue.ResourceId));
            }
        }

        protected override void OnDetached()
        {
            // Cleanup if needed
        }
    }
}
```

---

## Reusable Effects

### Creating Effect Extensions

Make effects easier to use with extension methods:

```csharp
// Effects/EffectExtensions.cs
namespace SweetShopMa.Effects
{
    public static class EffectExtensions
    {
        public static void AddShadow(this VisualElement element)
        {
            element.Effects.Add(new ShadowEffect());
        }

        public static void AddShadow(this VisualElement element, 
            Color color, float radius, float opacity)
        {
            var effect = new ShadowEffect
            {
                ShadowColor = color,
                ShadowRadius = radius,
                ShadowOpacity = opacity
            };
            element.Effects.Add(effect);
        }

        public static void AddFocus(this VisualElement element, 
            Color focusColor, Color unfocusedColor)
        {
            var effect = new FocusEffect
            {
                FocusColor = focusColor,
                UnfocusedColor = unfocusedColor
            };
            element.Effects.Add(effect);
        }
    }
}
```

**Usage:**
```csharp
// Easy to use in C#
var button = new Button { Text = "Click Me" };
button.AddShadow();

var entry = new Entry { Placeholder = "Enter text" };
entry.AddFocus(Colors.Blue, Colors.Gray);
```

---

## Common Use Cases

### 1. Label with Custom Font

```csharp
// Effects/CustomLabelEffect.cs
public class CustomLabelEffect : RoutingEffect
{
    public const string EffectName = "SweetShopMa.CustomLabelEffect";
    public string CustomFont { get; set; }
    
    public CustomLabelEffect() : base(EffectName) { }
}
```

### 2. Entry with Clear Button

```csharp
// Effects/ClearButtonEffect.cs
public class ClearButtonEffect : RoutingEffect
{
    public const string EffectName = "SweetShopMa.ClearButtonEffect";
    public bool ShowClearButton { get; set; } = true;
    
    public ClearButtonEffect() : base(EffectName) { }
}
```

### 3. Button with Touch Sound

```csharp
// Effects/TouchSoundEffect.cs
public class TouchSoundEffect : RoutingEffect
{
    public const string EffectName = "SweetShopMa.TouchSoundEffect";
    public string SoundFile { get; set; }
    
    public TouchSoundEffect() : base(EffectName) { }
}
```

---

## Best Practices

### 1. **Always Use Try-Catch**
```csharp
protected override void OnAttached()
{
    try
    {
        // Your effect code
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Effect error: {ex.Message}");
    }
}
```

### 2. **Check for Null**
```csharp
if (Control != null && Element != null)
{
    // Safe to proceed
}
```

### 3. **Clean Up in OnDetached**
```csharp
protected override void OnDetached()
{
    // Remove event handlers
    // Release resources
    // Reset properties
}
```

### 4. **Use ResolutionGroupName**
```csharp
[assembly: ResolutionGroupName("YourCompany")]
```

### 5. **Export Effects Properly**
```csharp
[assembly: ExportEffect(typeof(MyEffect), nameof(MyEffect))]
```

### 6. **Document Your Effects**
```csharp
/// <summary>
/// Adds a shadow effect to any VisualElement
/// Usage: element.Effects.Add(new ShadowEffect());
/// </summary>
public class ShadowEffect : RoutingEffect
{
    // ...
}
```

---

## Quick Reference

### Effect Lifecycle
```
OnAttached() → Effect is applied to control
    │
    ├─→ Control is visible and interactive
    │
OnDetached() → Effect is removed from control
```

### Key Methods
- `OnAttached()` - Called when effect is added to control
- `OnDetached()` - Called when effect is removed
- `OnElementPropertyChanged()` - Listen for property changes

### Important Properties
- `Element` - The MAUI control
- `Control` - The native platform control
- `Container` - The container view

---

## Summary

✅ **Effects** are lightweight alternatives to custom renderers  
✅ Perfect for **small visual tweaks** and platform-specific styling  
✅ Implement **OnAttached()** and **OnDetached()** methods  
✅ Always use **try-catch** blocks for error handling  
✅ Create **extension methods** for easier usage  

### Comparison with Custom Renderers

| Use Effects When... | Use Custom Renderers When... |
|---------------------|------------------------------|
| Just changing appearance | Replacing control behavior |
| Adding simple effects | Complex custom drawing |
| Minimal code needed | Full control required |
| Quick implementation | Deep platform integration |

### Next Steps
- Learn about **Animations** for dynamic UI effects
- Explore **Handlers** (modern MAUI approach)
- Study **Behaviors** for reusable interaction logic

---

**🎯 Exercise:** Create a `BorderEffect` that adds a customizable border to any control. Implement it for Android and iOS with properties for border color, width, and corner radius.

**📚 Resources:**
- [.NET MAUI Effects Documentation](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/effects)
- [Platform-Specific APIs](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/)
