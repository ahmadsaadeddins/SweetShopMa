# Custom Renderers in .NET MAUI - A Beginner's Guide

## Table of Contents
1. [What are Custom Renderers?](#what-are-custom-renderers)
2. [Why Use Custom Renderers?](#why-use-custom-renderers)
3. [Understanding the Architecture](#understanding-the-architecture)
4. [Your First Custom Renderer](#your-first-custom-renderer)
5. [Platform-Specific Implementation](#platform-specific-implementation)
6. [Best Practices](#best-practices)
7. [Common Scenarios](#common-scenarios)

---

## What are Custom Renderers?

**Custom Renderers** allow you to extend and customize the appearance and behavior of .NET MAUI controls on each platform (Android, iOS, Windows, MacCatalyst). They act as a bridge between your shared MAUI code and the native platform controls.

### Simple Analogy
Think of .NET MAUI controls as a **universal remote control** that works with all TV brands. Custom renderers are like **programming specific buttons** to work exactly how you want with each TV brand.

---

## Why Use Custom Renderers?

You need custom renderers when:
- ✅ You want to change the native appearance of a control
- ✅ You need to access platform-specific APIs
- ✅ The built-in MAUI properties don't meet your needs
- ✅ You want to add custom gestures or behaviors

### Example Scenarios
- Change the corner radius of a Button on iOS
- Add a shadow to an Entry on Android
- Create a custom progress bar with unique styling
- Implement a custom checkbox design

---

## Understanding the Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  SHARED MAUI CODE                        │
│  ┌───────────────────────────────────────────────────┐  │
│  │  MyCustomButton : Button                          │  │
│  │  - CustomProperty                                 │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                         │
                         │ Uses
                         ▼
┌─────────────────────────────────────────────────────────┐
│              PLATFORM-SPECIFIC RENDERERS                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   Android   │  │     iOS     │  │   Windows   │     │
│  │  Renderer   │  │  Renderer   │  │  Renderer   │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
                         │
                         │ Controls
                         ▼
┌─────────────────────────────────────────────────────────┐
│              NATIVE PLATFORM CONTROLS                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ AppCompat   │  │   UIButton  │  │  Windows.UI  │     │
│  │   Button    │  │             │  │   Controls   │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
```

---

## Your First Custom Renderer

### Step 1: Create the Custom Control

First, create a custom control in your shared project:

```csharp
// MyCustomButton.cs
namespace SweetShopMa.Controls
{
    public class MyCustomButton : Button
    {
        // Bindable property for custom corner radius
        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create(
                nameof(CornerRadius),
                typeof(int),
                typeof(MyCustomButton),
                5,
                propertyChanged: OnCornerRadiusChanged);

        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // This will be called when the property changes
            var control = (MyCustomButton)bindable;
            // The renderer will handle the actual visual update
        }
    }
}
```

### Step 2: Use the Control in XAML

```xml
<!-- MainPage.xaml -->
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:controls="clr-namespace:SweetShopMa.Controls"
    x:Class="SweetShopMa.Views.MainPage">

    <controls:MyCustomButton 
        Text="Click Me!"
        CornerRadius="20"
        BackgroundColor="Purple"
        TextColor="White"
        HorizontalOptions="Center"
        VerticalOptions="Center"/>
</ContentPage>
```

---

## Platform-Specific Implementation

### Android Renderer

Create the renderer in the `Platforms/Android` folder:

```csharp
// Platforms/Android/CustomRenderers/MyCustomButtonRenderer.cs
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using SweetShopMa.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Handlers.Compatibility;

[assembly: ExportRenderer(typeof(MyCustomButton), typeof(MyCustomButtonRenderer))]
namespace SweetShopMa.Platforms.Android.CustomRenderers
{
    public class MyCustomButtonRenderer : ButtonRenderer
    {
        public MyCustomButtonRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Element != null)
            {
                var customButton = (MyCustomButton)Element;
                
                // Create a gradient drawable for rounded corners
                var gradientDrawable = new GradientDrawable();
                gradientDrawable.SetColor(Element.BackgroundColor.ToAndroid());
                gradientDrawable.SetCornerRadius(customButton.CornerRadius);
                
                // Set the drawable to the button
                Control.SetBackground(gradientDrawable);
                
                // Remove default padding
                Control.SetPadding(
                    Control.PaddingLeft,
                    Control.PaddingTop,
                    Control.PaddingRight,
                    Control.PaddingBottom);
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == MyCustomButton.CornerRadiusProperty.PropertyName)
            {
                if (Control != null && Element != null)
                {
                    var customButton = (MyCustomButton)Element;
                    var gradientDrawable = new GradientDrawable();
                    gradientDrawable.SetColor(Element.BackgroundColor.ToAndroid());
                    gradientDrawable.SetCornerRadius(customButton.CornerRadius);
                    Control.SetBackground(gradientDrawable);
                }
            }
        }
    }
}
```

### iOS Renderer

Create the renderer in the `Platforms/iOS` folder:

```csharp
// Platforms/iOS/CustomRenderers/MyCustomButtonRenderer.cs
using CoreGraphics;
using Foundation;
using SweetShopMa.Controls;
using UIKit;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Handlers.Compatibility;

[assembly: ExportRenderer(typeof(MyCustomButton), typeof(MyCustomButtonRenderer))]
namespace SweetShopMa.Platforms.iOS.CustomRenderers
{
    public class MyCustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Element != null)
            {
                var customButton = (MyCustomButton)Element;
                
                // Enable user interaction
                Control.UserInteractionEnabled = true;
                
                // Apply rounded corners
                UpdateCornerRadius();
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == MyCustomButton.CornerRadiusProperty.PropertyName)
            {
                UpdateCornerRadius();
            }
        }

        void UpdateCornerRadius()
        {
            if (Control == null || Element == null)
                return;

            var customButton = (MyCustomButton)Element;
            
            // Create rounded rectangle path
            var path = UIBezierPath.FromRoundedRect(
                Control.Bounds,
                UIRectCorner.AllCorners,
                new CGSize(customButton.CornerRadius, customButton.CornerRadius)
            );

            // Create shape layer
            var mask = new CAShapeLayer
            {
                Path = path.CGPath
            };

            Control.Layer.Mask = mask;
            Control.Layer.MasksToBounds = true;
        }
    }
}
```

### Windows Renderer

Create the renderer in the `Platforms/Windows` folder:

```csharp
// Platforms/Windows/CustomRenderers/MyCustomButtonRenderer.cs
using SweetShopMa.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

[assembly: ExportRenderer(typeof(MyCustomButton), typeof(MyCustomButtonRenderer))]
namespace SweetShopMa.Platforms.Windows.CustomRenderers
{
    public class MyCustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Element != null)
            {
                var customButton = (MyCustomButton)Element;
                UpdateCornerRadius();
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == MyCustomButton.CornerRadiusProperty.PropertyName)
            {
                UpdateCornerRadius();
            }
        }

        void UpdateCornerRadius()
        {
            if (Control == null || Element == null)
                return;

            var customButton = (MyCustomButton)Element;
            
            // Apply corner radius using ControlTemplate or Style
            Control.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(customButton.CornerRadius);
        }
    }
}
```

---

## Best Practices

### 1. **Always Check for Null**
```csharp
if (Control != null && Element != null)
{
    // Your code here
}
```

### 2. **Handle Property Changes**
```csharp
protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
{
    base.OnElementPropertyChanged(sender, e);

    if (e.PropertyName == MyCustomProperty.PropertyName)
    {
        UpdateVisuals();
    }
}
```

### 3. **Use ExportRenderer Attribute**
```csharp
[assembly: ExportRenderer(typeof(MyCustomControl), typeof(MyCustomControlRenderer))]
```

### 4. **Dispose Resources Properly**
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        // Clean up native resources
    }
    base.Dispose(disposing);
}
```

### 5. **Test on All Platforms**
Always test your custom renderers on each target platform to ensure consistent behavior.

---

## Common Scenarios

### Scenario 1: Custom Entry with Border

**Shared Code:**
```csharp
public class BorderEntry : Entry
{
    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(
            nameof(BorderColor),
            typeof(Color),
            typeof(BorderEntry),
            Colors.Gray);

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public static readonly BindableProperty BorderWidthProperty =
        BindableProperty.Create(
            nameof(BorderWidth),
            typeof(int),
            typeof(BorderEntry),
            1);

    public int BorderWidth
    {
        get => (int)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }
}
```

**Android Renderer:**
```csharp
protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
{
    base.OnElementChanged(e);

    if (Control != null && Element != null)
    {
        var borderEntry = (BorderEntry)Element;
        
        // Create gradient drawable for border
        var gd = new GradientDrawable();
        gd.SetColor(global::Android.Graphics.Color.White);
        gd.SetStroke(
            borderEntry.BorderWidth,
            borderEntry.BorderColor.ToAndroid());
        
        Control.Background = gd;
    }
}
```

### Scenario 2: Custom Frame with Shadow

**Shared Code:**
```csharp
public class ShadowFrame : Frame
{
    public static readonly BindableProperty HasShadowProperty =
        BindableProperty.Create(
            nameof(HasShadow),
            typeof(bool),
            typeof(ShadowFrame),
            false);

    public bool HasShadow
    {
        get => (bool)GetValue(HasShadowProperty);
        set => SetValue(HasShadowProperty, value);
    }
}
```

**iOS Renderer:**
```csharp
protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
{
    base.OnElementChanged(e);

    if (Control != null && Element != null)
    {
        var shadowFrame = (ShadowFrame)Element;
        UpdateShadow();
    }
}

void UpdateShadow()
{
    if (Control == null || Element == null)
        return;

    var shadowFrame = (ShadowFrame)Element;
    
    if (shadowFrame.HasShadow)
    {
        Control.Layer.ShadowColor = UIColor.Black.CGColor;
        Control.Layer.ShadowOffset = new CGSize(0, 2);
        Control.Layer.ShadowOpacity = 0.3f;
        Control.Layer.ShadowRadius = 4;
    }
    else
    {
        Control.Layer.ShadowOpacity = 0;
    }
}
```

---

## Quick Reference

### Key Methods to Override
- `OnElementChanged()` - Called when the element is created or changed
- `OnElementPropertyChanged()` - Called when a property changes
- `Dispose()` - Clean up native resources

### Important Properties
- `Element` - The MAUI control
- `Control` - The native platform control

### Common Namespaces
```csharp
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Handlers.Compatibility;
```

---

## Summary

✅ **Custom Renderers** let you customize controls on each platform  
✅ Create a **shared control** with bindable properties  
✅ Implement **platform-specific renderers** for each target  
✅ Always check for **null** before accessing Control or Element  
✅ Handle **property changes** to update visuals dynamically  

### Next Steps
- Learn about **Effects** (lighter alternative to custom renderers)
- Explore **Animations** for engaging user experiences
- Study **Handlers** (the modern replacement for renderers in MAUI)

---

**🎯 Exercise:** Create a custom `GradientButton` that displays a gradient background instead of a solid color. Implement it for Android and iOS.

**📚 Resources:**
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Custom Renderers Guide](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/custom-renderers)
