---
description: Build the .NET MAUI SweetShopMa application
---

# Build MAUI Application

Build the SweetShopMa .NET MAUI application for different platforms.

## Build for Windows

1. Navigate to the project directory:
```bash
cd c:\Users\AMA\Documents\myhub\SweetShopMa\SweetShopMa
```

// turbo
2. Build for Windows (Debug):
```bash
dotnet build -f net10.0-windows10.0.19041.0
```

3. Build for Windows (Release):
```bash
dotnet build -f net10.0-windows10.0.19041.0 -c Release
```

## Build for Android

// turbo
4. Build for Android (Debug):
```bash
dotnet build -f net10.0-android
```

5. Build for Android (Release):
```bash
dotnet build -f net10.0-android -c Release
```

## Publish Android APK

6. Publish Android APK:
```bash
dotnet publish -f net10.0-android -c Release
```

## Clean and Rebuild

// turbo
7. Clean the solution:
```bash
dotnet clean
```

// turbo
8. Restore packages:
```bash
dotnet restore
```
