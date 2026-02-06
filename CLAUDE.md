# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

SweetShopMa is a Point of Sale (POS) system with two implementations:
1. **MAUI App** (`SweetShopMa/`) - Cross-platform .NET MAUI application for Windows, Android, iOS
2. **Desktop App** (`sweetshopma-desktop/`) - Python-based desktop app using Django + PyWebView/React

---

## .NET MAUI Application (`SweetShopMa/`)

### Build Commands

```bash
# Navigate to project directory
cd SweetShopMa

# Build for Windows (Debug)
dotnet build -f net10.0-windows10.0.19041.0

# Build for Windows (Release)
dotnet build -f net10.0-windows10.0.19041.0 -c Release

# Build for Android (Debug)
dotnet build -f net10.0-android

# Run on Windows
dotnet run -f net10.0-windows10.0.19041.0

# Run with Hot Reload
dotnet watch run -f net10.0-windows10.0.19041.0

# Clean solution
dotnet clean
```

### Architecture

**MVVM Pattern with CommunityToolkit.Mvvm**
- **Models**: Data entities (`Models/`) - Product, User, Order, AttendanceRecord, etc.
- **ViewModels**: Business logic (`ViewModels/`) - inherit from `BaseViewModel`
- **Views**: XAML UI (`Views/`) - data bound to ViewModels

**Dependency Injection** (configured in `MauiProgram.cs`)
- **Singleton services**: DatabaseService, AuthService, CartService, LocalizationService, PdfService, ExportService
- **Singleton ViewModels**: ShopViewModel, AdminViewModel, UserManagementViewModel, ProductManagementViewModel, AttendanceViewModel, ReportsViewModel, MenuBarViewModel
- **Transient ViewModels**: SettingsViewModel, LocationsViewModel, StockTransferViewModel
- **Views**: MainPage is singleton, others are transient

**Database** (`DatabaseService.cs`)
- SQLite database at: `%AppData%\Local\Packages\[AppName]\LocalState\sweetshop.db3`
- Uses WAL mode for concurrency
- Thread-safe initialization with SemaphoreSlim

**Role-Based Access Control** (`RoleConstants.cs`)
- Roles: Developer, Admin, Moderator, User
- Permissions defined in UserProfile model and enforced in ViewModels

**Localization** (`Services/LocalizationService.cs`)
- Resources: `Resources/Strings.resx` (English), `Resources/Strings.ar.resx` (Arabic)
- Use XAML markup extension: `{localization:LocalizedString Name=YourKey}`

**Key Services**
- `AuthService`: User authentication and session management
- `CartService`: Shopping cart state management
- `AttendanceRulesService`: Business rules for attendance/payroll
- `ExportService`: Excel/CSV/PDF export generation
- `PdfService`: PDF generation using QuestPDF

**Platform-Specific Services** (Windows only)
- `WindowsPrintService`: HTML-based printing
- `WindowsCashDrawerService`: ESC/POS cash drawer control
- `WindowsKeyboardService`: Global keyboard hook handling

### Code Conventions

- Use `[ObservableProperty]` for auto-generated properties in ViewModels
- Use `[RelayCommand]` for auto-generated commands
- All ViewModels inherit from `BaseViewModel` which provides IsBusy, StatusMessage, ShowStatus()
- Call `_loggingService.LogDebug()` for debugging in production
- Use `MainThread.InvokeOnMainThreadAsync()` for UI updates from background threads

---

## Desktop Application (`sweetshopma-desktop/`)

### Directory Structure

```
sweetshopma-desktop/
├── backend/              # Django REST API
│   ├── sweetshop/       # Django project settings
│   └── api/             # API app (models, views, serializers)
├── frontend/            # React + Vite frontend
│   ├── src/            # React source
│   ├── templates/      # Django templates (legacy)
│   └── build/          # Production build output
├── security/           # Licensing and encryption
├── sync/              # Multi-branch sync service
├── config/            # Application configuration
└── scripts/           # Build and utility scripts
```

### Development Commands

```bash
# Backend (Django)
cd sweetshopma-desktop/backend
python manage.py runserver
python manage.py migrate
python manage.py createsuperuser
python manage.py test

# Frontend (React + Vite)
cd sweetshopma-desktop/frontend
npm install
npm run dev        # Development server
npm run build      # Production build

# Run full desktop app
cd sweetshopma-desktop
python frontend/main.py           # Production mode
python frontend/main.py --dev     # Development mode (requires npm run dev)
```

### Architecture

**Backend (Django + DRF)**
- REST API at `http://127.0.0.1:8000/api/`
- Models: Product, Sale, SaleItem, Expense, UserProfile, AttendanceRecord, etc.
- SQLCipher database backend for encryption
- Role-based permissions via UserProfile

**Frontend Options**
1. **React + Vite** (current/primary) - Modern SPA in `frontend/src/`
2. **Django Templates** (legacy) - ES5 JavaScript required due to PyWebView limitations

**PyWebView Integration**
- Python backend (`frontend/main.py`) starts Django in background thread
- PyWebView window loads React app (dev server or built files)
- `frontend/api.py` provides ApiBridge class exposed to JavaScript as `pywebview.api`

**Security**
- Hardware-based licensing (`security/license_manager.py`)
- SQLCipher encryption (`security/key_manager.py`)
- Anti-debugging protection (`security/anti_debug.py`)

### PyWebView Critical Rules

From `.kilocode/rules/PYWEBVIEW_DEBUGGING_RULES.md`:

1. **ES5 Only JavaScript** - No const/let, arrow functions, template literals, spread operator
2. **Script Execution** - Scripts in innerHTML don't auto-execute; manually extract and run
3. **CORS Required** - Install `django-cors-headers` and configure settings
4. **API Response Format** - Handle both `Array.isArray(response)` AND `response.results`
5. **DRF Serializers** - All declared fields must be in `Meta.fields`
6. **No Invalid Settings** - Never reference non-existent modules in settings.py

---

## Shared Domain Concepts

Both implementations share the same business domain:

### Core Entities
- **Product**: Inventory items with price, cost, quantity, barcode/SKU
- **Sale/SaleItem**: Transactions with payment methods, discounts, line items
- **User/UserProfile**: Staff with roles (Developer/Admin/Moderator/User) and salary info
- **AttendanceRecord**: Employee attendance with hours worked, check-in/out times
- **AttendanceExpense**: Employee expenses for payroll deduction
- **RestockRecord**: Audit trail for inventory restocking
- **Expense**: Business expenses by category

### Business Logic
- Attendance tracking with 6-day work week and absence deductions
- Salary calculations with overtime multipliers
- Low stock alerts and restock reporting
- Multi-location/branch support
- Offline-first operation with sync to central server (desktop)

---

## Common Issues

### MAUI
- Database file location: Use `DatabaseService.DatabasePath` property
- Localization: Add keys to both Strings.resx and Strings.ar.resx
- Hot Reload: Works for XAML changes, C# changes require rebuild

### Desktop
- React build required before running production mode
- ES5 JavaScript required for PyWebView compatibility (see rules above)
- CORS errors: Add `corsheaders.middleware.CorsMiddleware` first in MIDDLEWARE
- Database locked: Close all connections before running migrations
