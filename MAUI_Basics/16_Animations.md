# Animations in .NET MAUI - A Beginner's Guide

## Table of Contents
1. [What are Animations?](#what-are-animations)
2. [Why Use Animations?](#why-use-animations)
3. [Animation Basics](#animation-basics)
4. [Built-in Animations](#built-in-animations)
5. [Compound Animations](#compound-animations)
6. [Custom Animations](#custom-animations)
7. [Animation Easing](#animation-easing)
8. [Practical Examples](#practical-examples)
9. [Best Practices](#best-practices)

---

## What are Animations?

**Animations** in .NET MAUI allow you to create smooth, dynamic visual effects that enhance user experience. They can make your app feel more responsive, engaging, and polished.

### Simple Analogy
Think of animations like **movie special effects** - they make static images come alive with movement, transitions, and visual feedback.

### What Can You Animate?
- 🎯 **Position** - Move elements around the screen
- 📏 **Size** - Scale elements up or down
- 🔄 **Rotation** - Rotate elements
- 🎨 **Opacity** - Fade elements in or out
- 📐 **Layout** - Animate layout changes

---

## Why Use Animations?

✅ **Better User Experience**
- Guide user attention to important elements
- Provide visual feedback for actions
- Make the app feel more responsive

✅ **Professional Polish**
- Smooth transitions between screens
- Engaging loading states
- Delightful micro-interactions

✅ **Communication**
- Show cause and effect relationships
- Indicate loading or processing
- Provide visual hierarchy

⚠️ **Use Responsibly**
- Don't overuse animations
- Keep them subtle and purposeful
- Respect user accessibility preferences

---

## Animation Basics

### The Animation Class

The [`ViewExtensions`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.viewextensions) class provides extension methods for animating visual elements.

### Basic Animation Pattern

```csharp
// All animations follow this pattern:
await element.AnimatePropertyAsync(
    value,           // Target value
    length,          // Duration (uint)
    easing           // Optional: Easing function
);
```

### Example: Fade In Animation

```csharp
// Start invisible
myLabel.Opacity = 0;

// Fade in over 1 second
await myLabel.FadeTo(1, 1000);
```

---

## Built-in Animations

### 1. FadeTo - Opacity Animation

Gradually change the opacity of an element.

```csharp
// Fade out
await myButton.FadeTo(0, 500);

// Fade in
await myButton.FadeTo(1, 500);

// Fade to 50% opacity
await myButton.FadeTo(0.5, 300);
```

**Practical Example:**
```csharp
// Show a loading indicator
loadingIndicator.IsVisible = true;
loadingIndicator.Opacity = 0;
await loadingIndicator.FadeTo(1, 250);

// Hide after work is done
await loadingIndicator.FadeTo(0, 250);
loadingIndicator.IsVisible = false;
```

### 2. ScaleTo - Size Animation

Scale an element up or down.

```csharp
// Scale up to 1.5x
await myImage.ScaleTo(1.5, 500);

// Scale down to 0.5x
await myImage.ScaleTo(0.5, 500);

// Return to normal size
await myImage.ScaleTo(1, 500);
```

**Practical Example:**
```csharp
// Button press effect
async void OnButtonClicked(object sender, EventArgs e)
{
    var button = (Button)sender;
    
    // Shrink slightly
    await button.ScaleTo(0.95, 50);
    
    // Return to normal
    await button.ScaleTo(1, 50);
}
```

### 3. RotateTo - Rotation Animation

Rotate an element around its center.

```csharp
// Rotate 90 degrees
await myLabel.RotateTo(90, 500);

// Rotate 180 degrees
await myLabel.RotateTo(180, 1000);

// Rotate back to 0
await myLabel.RotateTo(0, 500);
```

**Practical Example:**
```csharp
// Loading spinner
bool isLoading = true;

while (isLoading)
{
    await loadingImage.RotateTo(360, 1000, Easing.Linear);
    loadingImage.Rotation = 0; // Reset for next rotation
}
```

### 4. TranslateTo - Position Animation

Move an element to a new position.

```csharp
// Move right by 100 pixels
await myButton.TranslateTo(100, 0, 500);

// Move down by 50 pixels
await myButton.TranslateTo(0, 50, 500);

// Move diagonally
await myButton.TranslateTo(100, 100, 500);

// Return to original position
await myButton.TranslateTo(0, 0, 500);
```

**Practical Example:**
```csharp
// Slide in from right
myButton.TranslationX = 200; // Start off-screen
await myButton.TranslateTo(0, 0, 500, Easing.CubicOut);
```

### 5. RelRotateTo - Relative Rotation

Rotate by a specified amount relative to current rotation.

```csharp
// Rotate 45 degrees from current position
await myImage.RelRotateTo(45, 500);

// Keep spinning
while (true)
{
    await myImage.RelRotateTo(360, 2000, Easing.Linear);
}
```

### 6. RelScaleTo - Relative Scaling

Scale by a specified amount relative to current scale.

```csharp
// Double the current size
await myImage.RelScaleTo(2, 500);

// Increase by 50%
await myImage.RelScaleTo(1.5, 500);
```

---

## Compound Animations

### Sequential Animations

Run animations one after another.

```csharp
// Fade in, then scale up
await myLabel.FadeTo(1, 500);
await myLabel.ScaleTo(1.2, 500);
```

### Parallel Animations

Run multiple animations at the same time.

```csharp
// Fade and scale simultaneously
var fadeTask = myLabel.FadeTo(1, 500);
var scaleTask = myLabel.ScaleTo(1.2, 500);

await Task.WhenAll(fadeTask, scaleTask);
```

### Complex Animation Sequence

```csharp
async Task AnimateButton()
{
    var button = new Button { Text = "Animate Me" };
    
    // Shrink
    await button.ScaleTo(0.8, 100);
    
    // Rotate while shrinking
    var rotateTask = button.RotateTo(360, 500, Easing.Linear);
    var fadeTask = button.FadeTo(0.5, 500);
    await Task.WhenAll(rotateTask, fadeTask);
    
    // Reset
    button.Rotation = 0;
    await button.ScaleTo(1, 200);
    await button.FadeTo(1, 200);
}
```

---

## Custom Animations

### Creating Custom Animations

Use the [`Animation`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.animation) class for complex animations.

#### Basic Custom Animation

```csharp
// Create a custom animation
var animation = new Animation(
    callback: v => myLabel.Scale = v,  // What to animate
    start: 1,                           // Start value
    end: 1.5,                           // End value
    easing: Easing.CubicInOut           // Easing function
);

// Run the animation
animation.Commit(
    owner: myLabel,                     // Element to animate
    name: "ScaleAnimation",             // Animation name
    length: 1000                        // Duration in ms
);
```

#### Advanced Custom Animation

```csharp
// Animate multiple properties
var animation = new Animation();

// Add child animations
animation.Add(0, 1, new Animation(
    v => myLabel.Scale = v,
    1, 1.5,
    Easing.CubicOut
));

animation.Add(0, 1, new Animation(
    v => myLabel.Rotation = v,
    0, 360,
    Easing.Linear
));

animation.Add(0, 1, new Animation(
    v => myLabel.Opacity = v,
    1, 0.5,
    Easing.SinInOut
));

// Commit with callback
animation.Commit(
    owner: myLabel,
    name: "ComplexAnimation",
    length: 2000,
    finished: (v, cancelled) =>
    {
        // Animation completed
        myLabel.Scale = 1;
        myLabel.Rotation = 0;
        myLabel.Opacity = 1;
    }
);
```

### Animate Custom Properties

```csharp
// Animate a custom property
public class MyCustomView : ContentView
{
    public double CustomValue { get; set; }
    
    public async Task AnimateCustomValue()
    {
        var animation = new Animation(
            v => CustomValue = v,
            0, 100,
            Easing.Linear
        );
        
        animation.Commit(this, "CustomAnimation", 1000);
    }
}
```

---

## Animation Easing

Easing functions control the acceleration and deceleration of animations.

### Built-in Easing Functions

```csharp
// Linear - Constant speed
await button.ScaleTo(1.5, 1000, Easing.Linear);

// BounceIn - Bounces at the start
await button.ScaleTo(1.5, 1000, Easing.BounceIn);

// BounceOut - Bounces at the end
await button.ScaleTo(1.5, 1000, Easing.BounceOut);

// CubicIn - Accelerates
await button.ScaleTo(1.5, 1000, Easing.CubicIn);

// CubicOut - Decelerates
await button.ScaleTo(1.5, 1000, Easing.CubicOut);

// CubicInOut - Accelerates then decelerates
await button.ScaleTo(1.5, 1000, Easing.CubicInOut);

// SinIn - Smooth acceleration
await button.ScaleTo(1.5, 1000, Easing.SinIn);

// SinOut - Smooth deceleration
await button.ScaleTo(1.5, 1000, Easing.SinOut);

// SinInOut - Smooth acceleration and deceleration
await button.ScaleTo(1.5, 1000, Easing.SinInOut);

// Spring - Spring-like motion
await button.ScaleTo(1.5, 1000, Easing.SpringIn);
await button.ScaleTo(1.5, 1000, Easing.SpringOut);
```

### Visual Comparison

```
Linear:      ━━━━━━━━━━━━━━━━━━━━━━━━
BounceOut:   ━━━━━━━━━━━━━━━━━━┅┅┅┅┅┅
CubicOut:    ━━━━━━━━━━━━━━━━━━━━━━━┓
CubicInOut:  ━┓━━━━━━━━━━━━━━━━━━━┓━
SpringOut:   ━━━━━━━━━━━━━━━━━━━━━┅┅
```

### Creating Custom Easing Functions

```csharp
// Custom easing function
public class CustomEasing : Easing
{
    public CustomEasing() : base(v => v * v)
    {
    }
}

// Use it
await button.ScaleTo(1.5, 1000, new CustomEasing());
```

---

## Practical Examples

### Example 1: Page Transition Animation

```csharp
// Slide in new page from right
public class SlidePage : ContentPage
{
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Start off-screen
        Content.TranslationX = 200;
        
        // Slide in
        await Content.TranslateTo(0, 0, 500, Easing.CubicOut);
    }
}
```

### Example 2: Loading Animation

```csharp
// Animated loading spinner
public class LoadingSpinner : ContentView
{
    Image spinnerImage;
    bool isLoading;
    
    public LoadingSpinner()
    {
        spinnerImage = new Image
        {
            Source = "spinner.png",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        
        Content = spinnerImage;
    }
    
    public async Task StartLoading()
    {
        isLoading = true;
        IsVisible = true;
        
        while (isLoading)
        {
            await spinnerImage.RotateTo(360, 1000, Easing.Linear);
            spinnerImage.Rotation = 0;
        }
    }
    
    public void StopLoading()
    {
        isLoading = false;
        IsVisible = false;
    }
}
```

### Example 3: Button Press Effect

```csharp
// Reusable button press animation
public static class ButtonAnimations
{
    public static async Task PressEffect(this Button button)
    {
        await button.ScaleTo(0.95, 50, Easing.CubicOut);
        await button.ScaleTo(1, 50, Easing.CubicIn);
    }
    
    public static async Task SuccessEffect(this Button button)
    {
        var originalColor = button.BackgroundColor;
        
        // Flash green
        button.BackgroundColor = Colors.Green;
        await button.FadeTo(0.5, 100);
        await button.FadeTo(1, 100);
        
        // Return to normal
        button.BackgroundColor = originalColor;
    }
}

// Usage
async void OnSaveClicked(object sender, EventArgs e)
{
    var button = (Button)sender;
    await button.PressEffect();
    
    // Perform save operation
    await SaveData();
    
    await button.SuccessEffect();
}
```

### Example 4: Card Flip Animation

```csharp
// Flip a card to reveal content
public class FlipCard : ContentView
{
    Frame frontFrame;
    Frame backFrame;
    bool isFlipped;
    
    public FlipCard()
    {
        frontFrame = new Frame
        {
            BackgroundColor = Colors.Blue,
            Content = new Label { Text = "Front", TextColor = Colors.White }
        };
        
        backFrame = new Frame
        {
            BackgroundColor = Colors.Red,
            Content = new Label { Text = "Back", TextColor = Colors.White },
            IsVisible = false
        };
        
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) => await Flip();
        frontFrame.GestureRecognizers.Add(tapGesture);
        
        Content = new Grid
        {
            Children = { frontFrame, backFrame }
        };
    }
    
    public async Task Flip()
    {
        if (isFlipped)
        {
            // Flip back
            await frontFrame.RotateYTo(0, 500, Easing.CubicInOut);
            backFrame.IsVisible = false;
            frontFrame.IsVisible = true;
        }
        else
        {
            // Flip to back
            await frontFrame.RotateYTo(180, 500, Easing.CubicInOut);
            frontFrame.IsVisible = false;
            backFrame.IsVisible = true;
            backFrame.RotationY = 180;
            await backFrame.RotateYTo(0, 500, Easing.CubicInOut);
        }
        
        isFlipped = !isFlipped;
    }
}
```

### Example 5: Progress Bar Animation

```csharp
// Animated progress bar
public class AnimatedProgressBar : BoxView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(
            nameof(Progress),
            typeof(double),
            typeof(AnimatedProgressBar),
            0.0,
            propertyChanged: OnProgressChanged);
    
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
    
    static async void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var progressBar = (AnimatedProgressBar)bindable;
        var newProgress = (double)newValue;
        
        // Animate to new progress
        await progressBar.LayoutTo(
            new Rect(0, 0, progressBar.Width * newProgress, progressBar.Height),
            500,
            Easing.CubicOut);
    }
}
```

---

## Best Practices

### 1. **Keep Animations Short**
```csharp
// Good - Quick feedback
await button.ScaleTo(0.95, 50);

// Bad - Too slow
await button.ScaleTo(0.95, 1000);
```

### 2. **Use Appropriate Easing**
```csharp
// Natural movement
await element.TranslateTo(100, 0, 500, Easing.CubicOut);

// Mechanical movement
await element.TranslateTo(100, 0, 500, Easing.Linear);
```

### 3. **Cancel Animations When Needed**
```csharp
// Cancel named animation
myLabel.AbortAnimation("FadeAnimation");

// Cancel all animations
ViewExtensions.CancelAnimations(myLabel);
```

### 4. **Handle Animation Completion**
```csharp
await myLabel.FadeTo(1, 500);

// Code here runs after animation completes
myLabel.IsVisible = true;
```

### 5. **Test on Real Devices**
- Emulators may not show smooth animations
- Test on actual devices for accurate performance
- Consider lower-end devices

### 6. **Respect Accessibility**
```csharp
// Check if user prefers reduced motion
if (AccessibilityPreferences.IsReduceMotionEnabled)
{
    // Skip animations or use instant transitions
    myLabel.Opacity = 1;
}
else
{
    await myLabel.FadeTo(1, 500);
}
```

### 7. **Avoid Animation Overload**
```csharp
// Bad - Too many animations at once
async void OnButtonClicked(object sender, EventArgs e)
{
    await button.ScaleTo(1.2, 200);
    await button.RotateTo(360, 200);
    await button.FadeTo(0.5, 200);
    await button.TranslateTo(100, 0, 200);
    // Too much!
}

// Good - Subtle, purposeful animation
async void OnButtonClicked(object sender, EventArgs e)
{
    await button.ScaleTo(0.95, 50);
    await button.ScaleTo(1, 50);
}
```

---

## Performance Tips

### 1. **Use Hardware Acceleration**
```xml
<!-- In your platform-specific projects -->
<application android:hardwareAccelerated="true">
```

### 2. **Animate Transform Properties**
- `Scale`, `Rotation`, `Translation` are GPU-accelerated
- `Width`, `Height`, `Opacity` may trigger layout calculations

### 3. **Batch Animations**
```csharp
// Good - Parallel animations
await Task.WhenAll(
    element1.FadeTo(1, 500),
    element2.ScaleTo(1.2, 500)
);

// Bad - Sequential when not needed
await element1.FadeTo(1, 500);
await element2.ScaleTo(1.2, 500);
```

### 4. **Avoid Layout Changes During Animation**
```csharp
// Bad - Changing layout during animation
await element.TranslateTo(100, 0, 500);
element.WidthRequest = 200; // Triggers layout recalculation

// Good - Complete animation first
await element.TranslateTo(100, 0, 500);
element.WidthRequest = 200;
```

---

## Quick Reference

### Common Animation Methods
| Method | Description | Example |
|--------|-------------|---------|
| `FadeTo()` | Change opacity | `await label.FadeTo(1, 500)` |
| `ScaleTo()` | Change size | `await button.ScaleTo(1.5, 500)` |
| `RotateTo()` | Rotate element | `await image.RotateTo(90, 500)` |
| `TranslateTo()` | Move element | `await button.TranslateTo(100, 0, 500)` |
| `RelRotateTo()` | Relative rotation | `await image.RelRotateTo(45, 500)` |
| `RelScaleTo()` | Relative scaling | `await image.RelScaleTo(1.5, 500)` |

### Common Easing Functions
| Function | Effect |
|----------|--------|
| `Linear` | Constant speed |
| `CubicOut` | Decelerates |
| `CubicInOut` | Accelerates then decelerates |
| `BounceOut` | Bounces at end |
| `SpringOut` | Spring-like motion |

### Animation Duration Guidelines
- **Micro-interactions**: 50-150ms
- **Feedback animations**: 200-300ms
- **Transitions**: 300-500ms
- **Loading animations**: 500-1000ms

---

## Summary

✅ **Animations** enhance user experience with smooth visual effects  
✅ Use **built-in methods** for common animations  
✅ Combine animations with **Task.WhenAll** for parallel execution  
✅ Choose appropriate **easing functions** for natural motion  
✅ Keep animations **short and purposeful**  
✅ Always test on **real devices**  

### Animation Checklist
- [ ] Animation serves a clear purpose
- [ ] Duration is appropriate (200-500ms)
- [ ] Easing function matches the interaction
- [ ] Tested on real devices
- [ ] Accessibility preferences respected
- [ ] Performance is acceptable

### Next Steps
- Learn about **Custom Renderers** for platform-specific animations
- Explore **Behaviors** for reusable interaction logic
- Study **Visual States** for state-based animations

---

**🎯 Exercise:** Create a custom loading spinner that rotates continuously and includes a pulsing effect. Add a method to smoothly start and stop the animation.

**🎯 Exercise:** Implement a card swipe animation where a card can be swiped left or right with different visual feedback for each direction.

**📚 Resources:**
- [.NET MAUI Animation Documentation](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/animation)
- [Easing Functions](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/animation/easing)
- [ViewExtensions Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.viewextensions)
