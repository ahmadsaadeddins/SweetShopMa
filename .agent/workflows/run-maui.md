---
description: Run the .NET MAUI SweetShopMa application locally
---

# Run MAUI Application

Run the SweetShopMa .NET MAUI application on different platforms.

## Run on Windows

1. Navigate to the project directory:
```bash
cd c:\Users\AMA\Documents\myhub\SweetShopMa\SweetShopMa
```

// turbo
2. Run on Windows:
```bash
dotnet run -f net10.0-windows10.0.19041.0
```

## Run on Android Emulator

3. Make sure Android emulator is running, then:
```bash
dotnet build -f net10.0-android -t:Run
```

## Hot Reload Development

// turbo
4. Run with Hot Reload enabled:
```bash
dotnet watch run -f net10.0-windows10.0.19041.0
```
