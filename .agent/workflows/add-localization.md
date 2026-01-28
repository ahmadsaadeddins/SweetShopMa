---
description: Add new localization strings for the MAUI application
---

# Add Localization Strings

Add new localization strings for English and Arabic in the .NET MAUI application.

## Localization Files Location

- **English:** `SweetShopMa/Resources/Strings.resx`
- **Arabic:** `SweetShopMa/Resources/Strings.ar.resx`

## Adding New Strings

1. Open `SweetShopMa/Resources/Strings.resx` and add the English string:
```xml
<data name="YourKeyName" xml:space="preserve">
  <value>English text here</value>
</data>
```

2. Open `SweetShopMa/Resources/Strings.ar.resx` and add the Arabic translation:
```xml
<data name="YourKeyName" xml:space="preserve">
  <value>النص العربي هنا</value>
</data>
```

3. Rebuild the project to generate the `Strings.Designer.cs` file:
```bash
cd c:\Users\AMA\Documents\myhub\SweetShopMa\SweetShopMa
dotnet build -f net10.0-windows10.0.19041.0
```

## Using Localized Strings in Code

In ViewModel or Code-Behind:
```csharp
string localizedText = _localizationService.GetString("YourKeyName");
```

In XAML:
```xml
<Label Text="{Binding LocalizedStrings[YourKeyName]}" />
```

## For Dynamic Language Switching

Update UI elements in code-behind when language changes:
```csharp
private void UpdateLocalizedStrings()
{
    YourLabel.Text = _localizationService.GetString("YourKeyName");
}
```

## Web App Localization

For the Next.js frontend, add strings to:
`web-app/frontend/lib/localization-context.tsx`
