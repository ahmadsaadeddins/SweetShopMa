# SweetShopMa - Complete Beginner Course

## Table of Contents

1. [Introduction & Overview](#1-introduction--overview)
2. [Project Structure](#2-project-structure)
3. [Core Concepts for Beginners](#3-core-concepts-for-beginners)
4. [Models (Data Structures)](#4-models-data-structures)
5. [Services (Business Logic)](#5-services-business-logic)
6. [ViewModels (UI Logic)](#6-viewmodels-ui-logic)
7. [Views (User Interface)](#7-views-user-interface)
8. [Application Lifecycle](#8-application-lifecycle)
9. [Platform-Specific Code](#9-platform-specific-code)
10. [Utilities & Helpers](#10-utilities--helpers)
11. [Resources](#11-resources)
12. [Common Patterns & Best Practices](#12-common-patterns--best-practices)

---

## 1. Introduction & Overview

### What is This Application?

**SweetShopMa** is a Point of Sale (POS) system built with .NET MAUI (Multi-platform App UI). It's designed for managing a sweet shop or retail store, handling:

- **Product Management**: Add, edit, and track inventory
- **Sales Processing**: Quick barcode scanning, cart management, checkout
- **User Management**: Role-based access control (Developer, Admin, Moderator, User)
- **Order Tracking**: Complete order history and reporting
- **Attendance Tracking**: Employee attendance management
- **Inventory Restocking**: Track who restocked what and when
- **Multi-language Support**: English and Arabic with RTL support
- **Receipt Printing**: Print receipts after checkout
- **Cash Drawer Control**: Open cash drawer via printer commands

### Technologies Used

- **.NET MAUI**: Cross-platform framework for building native apps (Windows, Android, iOS, Mac)
- **C#**: Programming language
- **XAML**: Markup language for defining UI layouts
- **SQLite**: Lightweight, embedded database for local data storage
- **MVVM Pattern**: Model-View-ViewModel architecture pattern

### Application Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    User Interface (Views)               │
│  LoginPage, MainPage, AdminPage, AttendancePage, etc.   │
└────────────────────┬────────────────────────────────────┘
                     │ Data Binding
┌────────────────────▼────────────────────────────────────┐
│              ViewModels (Business Logic)                │
│  ShopViewModel, AdminViewModel, RestockReportViewModel │
└────────────────────┬────────────────────────────────────┘
                     │ Uses
┌────────────────────▼────────────────────────────────────┐
│                  Services Layer                         │
│  DatabaseService, AuthService, CartService, etc.        │
└────────────────────┬────────────────────────────────────┘
                     │ Stores/Retrieves
┌────────────────────▼────────────────────────────────────┐
│              Models (Data Structures)                   │
│  Product, User, Order, CartItem, etc.                  │
└────────────────────┬────────────────────────────────────┘
                     │ Persisted in
┌────────────────────▼────────────────────────────────────┐
│              SQLite Database                            │
│              sweetshop.db3                              │
└─────────────────────────────────────────────────────────┘
```

### Key Features

1. **Role-Based Access Control**: Different user roles with specific permissions
2. **Barcode Scanning**: Quick product lookup and cart addition
3. **Weight-Based Products**: Support for products sold by weight (kilos)
4. **Real-time Inventory**: Stock tracking and validation
5. **Order Management**: Complete order history with details
6. **Localization**: English/Arabic support with automatic RTL layout
7. **Receipt Printing**: Generate and print receipts
8. **Cash Drawer Integration**: Open cash drawer via ESC/POS commands
9. **Attendance Tracking**: Employee attendance records and summaries
10. **Restock Tracking**: Audit trail for inventory restocking

---

## 2. Project Structure

### Folder Organization

```
SweetShopMa/
├── Models/              # Data structures (Product, User, Order, etc.)
├── Services/            # Business logic services (Database, Auth, Cart, etc.)
├── ViewModels/          # ViewModels for MVVM pattern
├── Views/               # UI pages (XAML + code-behind)
├── Platforms/           # Platform-specific code (Windows, Android, iOS)
│   └── Windows/         # Windows-specific implementations
├── Resources/           # Localization files (.resx)
├── Utils/               # Helper classes and utilities
└── App.xaml.cs          # Application entry point
```

### Purpose of Each Folder

- **Models/**: Contains data classes that represent entities in your application (Product, User, Order, etc.). These classes define the structure of data stored in the database.

- **Services/**: Contains service classes that handle business logic. Services are reusable components that perform specific tasks (database operations, authentication, cart management, etc.).

- **ViewModels/**: Contains ViewModel classes that connect Views (UI) with Models and Services. ViewModels expose data and commands that the UI can bind to.

- **Views/**: Contains XAML files (UI markup) and their code-behind files (C# event handlers). Each page in the app has a corresponding View.

- **Platforms/**: Contains platform-specific code. For example, Windows printing and cash drawer functionality is implemented here.

- **Resources/**: Contains localization resource files (Strings.resx for English, Strings.ar.resx for Arabic).

- **Utils/**: Contains utility classes and helper functions (password hashing, localization extensions, etc.).

### File Naming Conventions

- **Models**: PascalCase, singular (e.g., `Product.cs`, `User.cs`)
- **Services**: PascalCase, ends with "Service" (e.g., `DatabaseService.cs`, `AuthService.cs`)
- **ViewModels**: PascalCase, ends with "ViewModel" (e.g., `ShopViewModel.cs`)
- **Views**: PascalCase, ends with "Page" (e.g., `MainPage.xaml`, `LoginPage.xaml.cs`)
- **Interfaces**: PascalCase, starts with "I" (e.g., `IPrintService.cs`)

---

## 3. Core Concepts for Beginners

### What is MAUI?

**MAUI** (Multi-platform App UI) is a framework from Microsoft that allows you to write one codebase and deploy it to multiple platforms:
- Windows
- Android
- iOS
- macOS (Mac Catalyst)

Instead of writing separate apps for each platform, you write one app using C# and XAML, and MAUI handles the platform-specific details.

### MVVM Pattern Explained

**MVVM** stands for **Model-View-ViewModel**. It's a design pattern that separates concerns:

- **Model**: Represents data (Product, User, Order)
- **View**: The UI (XAML files, what the user sees)
- **ViewModel**: Connects Model and View, contains business logic

**Why MVVM?**
- Separates UI from business logic
- Makes code testable
- Allows UI to update automatically when data changes (data binding)

**Example Flow:**
```
User clicks "Add to Cart" button
    ↓
View (MainPage.xaml) triggers command
    ↓
ViewModel (ShopViewModel) executes AddToCartCommand
    ↓
ViewModel calls CartService
    ↓
CartService updates database
    ↓
ViewModel updates ObservableCollection
    ↓
View automatically updates (data binding)
```

### Dependency Injection Basics

**Dependency Injection (DI)** is a technique where objects receive their dependencies from outside rather than creating them internally.

**Why Use DI?**
- Makes code more testable
- Reduces coupling between classes
- Makes it easy to swap implementations

**Example:**
```csharp
// Without DI (bad):
public class ShopViewModel
{
    private DatabaseService _db = new DatabaseService(); // Hard to test
}

// With DI (good):
public class ShopViewModel
{
    private DatabaseService _db;
    
    public ShopViewModel(DatabaseService db) // Injected from outside
    {
        _db = db;
    }
}
```

**In MauiProgram.cs**, services are registered:
```csharp
.Services.AddSingleton<DatabaseService>()  // Register service
.AddSingleton<AuthService>()
.AddSingleton<ShopViewModel>()              // ViewModel gets services automatically
```

### SQLite Database Basics

**SQLite** is a lightweight, file-based database. It's perfect for mobile/desktop apps because:
- No server needed (runs locally)
- Single file database
- Fast and reliable

**In this app:**
- Database file: `sweetshop.db3` (stored in app data directory)
- Tables: User, Product, Order, OrderItem, CartItem, AttendanceRecord, RestockRecord
- Operations: Create, Read, Update, Delete (CRUD)

**Example:**
```csharp
// Create table
await _database.CreateTableAsync<Product>();

// Insert
await _database.InsertAsync(product);

// Query
var products = await _database.Table<Product>().ToListAsync();
```

### XAML Markup Language

**XAML** (eXtensible Application Markup Language) is used to define UI layouts.

**Basic Structure:**
```xml
<ContentPage>
    <StackLayout>
        <Label Text="Hello World" />
        <Button Text="Click Me" Clicked="OnButtonClicked" />
    </StackLayout>
</ContentPage>
```

**Data Binding:**
```xml
<!-- Bind to ViewModel property -->
<Label Text="{Binding CurrentUserName}" />

<!-- Bind to command -->
<Button Command="{Binding AddToCartCommand}" />
```

### Data Binding Concepts

**Data Binding** connects UI elements to data sources (ViewModels).

**Types of Binding:**
1. **One-way**: UI updates when data changes
   ```xml
   <Label Text="{Binding ProductName}" />
   ```

2. **Two-way**: UI and data update each other
   ```xml
   <Entry Text="{Binding SearchText, Mode=TwoWay}" />
   ```

3. **Command Binding**: UI triggers ViewModel methods
   ```xml
   <Button Command="{Binding AddToCartCommand}" />
   ```

### ObservableCollection and INotifyPropertyChanged

**ObservableCollection<T>**: A collection that notifies the UI when items are added, removed, or changed.

**INotifyPropertyChanged**: An interface that notifies the UI when a property value changes.

**Why They're Important:**
- Without them, UI won't update when data changes
- They enable automatic UI updates (data binding)

**Example:**
```csharp
public class ShopViewModel : INotifyPropertyChanged
{
    // ObservableCollection notifies UI when items change
    public ObservableCollection<Product> Products { get; } = new();
    
    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged(); // Notify UI to update
            }
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Commands and ICommand

**Commands** are a way to handle user actions (button clicks, etc.) in MVVM.

**Why Commands?**
- Separates UI from business logic
- Can enable/disable based on conditions
- Reusable across different UI elements

**Example:**
```csharp
public ICommand AddToCartCommand { get; }

public ShopViewModel()
{
    // Create command that executes AddToCart method
    AddToCartCommand = new Command<Product>(async product => 
    {
        await AddToCart(product);
    }, product => product != null); // CanExecute: only if product is not null
}
```

**In XAML:**
```xml
<Button Command="{Binding AddToCartCommand}" 
        CommandParameter="{Binding SelectedProduct}" />
```

---

## 4. Models (Data Structures)

Models represent the data structures used in the application. They define what information is stored in the database and how it's structured.

### Product.cs

**What it is:** Represents a product/item in the shop.

**Key Properties:**
- `Id`: Unique identifier (auto-incremented)
- `Name`: Product name (e.g., "Chocolate Bar")
- `Emoji`: Visual representation (e.g., "🍫")
- `Barcode`: Barcode for scanning
- `Price`: Price per unit/kilo
- `Stock`: Current inventory quantity
- `IsSoldByWeight`: Whether product is sold by weight (true) or by unit (false)

**Special Features:**
- Implements `INotifyPropertyChanged` for real-time UI updates
- `Quantity` property (not stored in DB, just for UI)
- `TotalPrice` computed property: `Price × Quantity`
- `UnitLabel` property: Returns "KGS" or "PCS" based on `IsSoldByWeight`

**Example Usage:**
```csharp
var product = new Product
{
    Name = "Chocolate Bar",
    Emoji = "🍫",
    Price = 5.50m,
    Stock = 100,
    IsSoldByWeight = false
};
```

### User.cs

**What it is:** Represents a user/employee in the system.

**Key Properties:**
- `Id`: Unique identifier
- `Username`: Login username
- `Password`: Hashed password (stored securely)
- `Name`: Full name
- `Role`: User role ("Developer", "Admin", "Moderator", "User")
- `IsEnabled`: Whether account is active
- `MonthlySalary`: Employee monthly salary
- `CreatedDate`: When account was created

**Permission Properties:**
- `IsDeveloper`, `IsAdmin`, `IsModerator`, `IsUser`: Role checks
- `CanManageUsers`: Developer or Admin only
- `CanManageStock`: Developer, Admin, or Moderator
- `CanUseAttendanceTracker`: Developer, Admin, or Moderator
- `CanRestock`: Developer, Admin, or Moderator

**Role Hierarchy:**
1. **Developer**: Full access, can create users
2. **Admin**: Can manage users, products, attendance, restock
3. **Moderator**: Can manage stock, attendance, restock (but not users)
4. **User**: Can only sell (use shop interface)

### CartItem.cs

**What it is:** Represents an item in the shopping cart.

**Key Properties:**
- `Id`: Unique identifier
- `ProductId`: Reference to Product
- `Name`, `Emoji`: Product details (denormalized for performance)
- `Price`: Price at time of adding to cart
- `Quantity`: How many/kilos in cart
- `IsSoldByWeight`: Whether sold by weight

**Computed Properties:**
- `ItemTotal`: `Price × Quantity`
- `UnitLabel`: "KGS" or "PCS"

**Special Features:**
- Implements `INotifyPropertyChanged` for automatic UI updates
- When `Quantity` or `Price` changes, `ItemTotal` automatically updates

### Order.cs

**What it is:** Represents a completed order/transaction.

**Key Properties:**
- `Id`: Unique identifier
- `UserId`: Who placed the order (cashier)
- `UserName`: Cashier name (for display)
- `OrderDate`: When order was placed
- `Total`: Total amount
- `ItemCount`: Number of items
- `Status`: Order status ("Completed", "Cancelled", etc.)

**Computed Properties:**
- `FormattedDate`: Formatted date string for display

### OrderItem.cs

**What it is:** Represents a single item within an order.

**Key Properties:**
- `Id`: Unique identifier
- `OrderId`: Reference to parent Order
- `ProductId`: Reference to Product
- `Name`, `Emoji`: Product details
- `Price`: Price at time of order
- `Quantity`: Quantity sold
- `IsSoldByWeight`: Whether sold by weight

**Computed Properties:**
- `ItemTotal`: `Price × Quantity`
- `UnitLabel`: "KGS" or "PCS"

**Relationship:**
- One Order can have many OrderItems (one-to-many)

### AttendanceRecord.cs

**What it is:** Represents an employee attendance record for a specific date.

**Key Properties:**
- `Id`: Unique identifier
- `UserId`: Employee ID
- `UserName`: Employee name
- `Date`: Attendance date
- `Status`: "Present" or "Absent"
- `IsPresent`: Boolean status
- `RegularHours`: Regular working hours
- `OvertimeHours`: Overtime hours
- `DailyPay`: Calculated daily pay
- `CheckInTime`, `CheckOutTime`: Time tracking
- `Notes`: Optional notes
- `CreatedAt`: Record creation timestamp

**Computed Properties:**
- `TotalHours`: `RegularHours + OvertimeHours`
- `CheckInDisplay`, `CheckOutDisplay`: Formatted time strings

### RestockRecord.cs

**What it is:** Represents a record of when inventory was restocked.

**Key Properties:**
- `Id`: Unique identifier
- `ProductId`: Which product was restocked
- `ProductName`, `ProductEmoji`: Product details
- `QuantityAdded`: How much was added
- `StockBefore`: Stock level before restock
- `StockAfter`: Stock level after restock
- `UserId`: Who performed the restock
- `UserName`: Name of person who restocked
- `RestockDate`: When restock occurred

**Purpose:** Provides an audit trail for inventory management.

---

## 5. Services (Business Logic)

Services contain the business logic of the application. They handle operations like database access, authentication, cart management, etc.

### DatabaseService.cs

**What it does:** Manages all database operations (CRUD - Create, Read, Update, Delete).

**Key Responsibilities:**
- Initialize SQLite database
- Create tables
- Insert, update, delete records
- Query data
- Seed initial data (default Developer user, sample products)

**Important Methods:**
- `InitializeAsync()`: Creates database and tables if they don't exist
- `GetProductsAsync()`: Retrieves all products
- `GetUserByUsernameAsync()`: Finds user by username
- `CreateOrderAsync()`: Creates a new order
- `UpdateProductStockAsync()`: Updates product inventory
- `SeedUsersAsync()`: Creates default Developer user if no users exist
- `SeedProductsAsync()`: Creates sample products if database is empty

**Thread Safety:**
- Uses `SemaphoreSlim` to ensure only one thread initializes the database at a time
- Uses WAL (Write-Ahead Logging) mode for better concurrency

**Example:**
```csharp
// Get all products
var products = await _databaseService.GetProductsAsync();

// Update product stock
await _databaseService.UpdateProductStockAsync(productId, -quantity);
```

### AuthService.cs

**What it does:** Handles user authentication and authorization.

**Key Responsibilities:**
- Login/logout
- Track current user
- Provide role and permission checks
- Notify when user changes

**Important Methods:**
- `LoginAsync(username, password)`: Validates credentials and logs in user
- `Logout()`: Clears current user

**Properties:**
- `CurrentUser`: Currently logged-in user
- `IsAuthenticated`: Whether a user is logged in
- `IsDeveloper`, `IsAdmin`, `IsModerator`, `IsUser`: Role checks
- `CanManageUsers`, `CanManageStock`, etc.: Permission checks

**Events:**
- `OnUserChanged`: Fired when user logs in/out

**Example:**
```csharp
// Login
bool success = await _authService.LoginAsync("username", "password");

// Check permissions
if (_authService.CanRestock)
{
    // Allow restocking
}
```

### CartService.cs

**What it does:** Manages the shopping cart.

**Key Responsibilities:**
- Add items to cart
- Remove items from cart
- Update quantities
- Calculate totals
- Checkout (create order, update inventory, clear cart)

**Important Methods:**
- `InitializeAsync()`: Loads cart from database
- `AddToCartAsync(product, quantity)`: Adds product to cart
- `RemoveFromCartAsync(item)`: Removes item from cart
- `UpdateCartItemQuantityAsync(item, newQuantity)`: Updates quantity
- `CheckoutAsync(userId, userName)`: Completes purchase

**Stock Validation:**
- Checks stock availability before adding/updating
- Returns `false` if insufficient stock

**Events:**
- `OnCartChanged`: Fired when cart contents change

**Example:**
```csharp
// Add to cart
bool success = await _cartService.AddToCartAsync(product, 2.5m);

// Checkout
var order = await _cartService.CheckoutAsync(userId, userName);
```

### LocalizationService.cs

**What it does:** Manages multi-language support (English/Arabic).

**Key Responsibilities:**
- Load localized strings from resource files
- Switch between languages
- Handle RTL (Right-to-Left) layout for Arabic
- Persist language preference

**Important Methods:**
- `GetString(key)`: Gets localized string by key
- `SetLanguage(languageCode)`: Changes language ("en" or "ar")

**Properties:**
- `CurrentCulture`: Current language culture
- `IsRTL`: Whether current language is RTL (Arabic)

**Events:**
- `LanguageChanged`: Fired when language changes

**Example:**
```csharp
// Get localized string
string text = _localizationService.GetString("Login");

// Change language
_localizationService.SetLanguage("ar"); // Arabic
```

### IPrintService & WindowsPrintService

**What it does:** Handles receipt printing.

**Interface:** `IPrintService` defines the contract
- `PrintReceiptAsync(receiptText, title)`: Prints receipt

**Windows Implementation:** `WindowsPrintService`
- Creates temporary HTML file
- Opens in default browser
- Auto-triggers print dialog via JavaScript
- Cleans up temporary file after printing

**Default Implementation:** `DefaultPrintService`
- Uses MAUI Share API as fallback for non-Windows platforms

### ICashDrawerService & WindowsCashDrawerService

**What it does:** Opens cash drawer via printer commands.

**Interface:** `ICashDrawerService` defines the contract
- `OpenDrawerAsync()`: Opens cash drawer

**Windows Implementation:** `WindowsCashDrawerService`
- Sends ESC/POS command to printer
- Tries multiple methods:
  1. Direct serial/parallel port communication
  2. Raw print command to default printer
- Uses `System.Management` to find printer port
- Uses `System.IO.Ports` for serial communication

**Default Implementation:** `DefaultCashDrawerService`
- Returns `false` (not supported) for non-Windows platforms

### FeatureFlags.cs

**What it does:** Provides feature toggles to enable/disable features.

**Current Flags:**
- `IsAttendanceTrackerEnabled`: Controls visibility of attendance tracker

**Purpose:** Allows easy feature management without code changes.

---

## 6. ViewModels (UI Logic)

ViewModels connect Views (UI) with Models and Services. They contain the business logic that the UI interacts with.

### ShopViewModel.cs

**What it does:** Manages the main shop interface (product browsing, cart, checkout).

**Key Responsibilities:**
- Display products
- Search/filter products
- Manage shopping cart
- Handle checkout
- Quick barcode entry
- Restock products
- Print receipts
- Open cash drawer

**Important Properties:**
- `Products`: All products (ObservableCollection)
- `FilteredProducts`: Products matching search (ObservableCollection)
- `CartItems`: Items in cart (ObservableCollection)
- `Total`: Cart total
- `QuickSearchText`: Barcode search text
- `SelectedQuickProduct`: Currently selected product for quick add
- `QuickQuantityText`: Quantity for quick add

**Important Commands:**
- `QuickAddCommand`: Adds product to cart via barcode
- `CheckoutCommand`: Completes purchase
- `RestockCommand`: Restocks a product
- `OpenDrawerCommand`: Opens cash drawer

**Key Methods:**
- `InitializeAsync()`: Loads products and cart
- `FilterProducts()`: Filters products based on search
- `QuickAddToCart()`: Adds product via barcode entry
- `Checkout()`: Handles checkout process
- `Restock()`: Restocks a product
- `PrintReceiptAsync()`: Generates and prints receipt
- `OpenDrawer()`: Opens cash drawer

**Focus Management:**
- Manages focus between barcode entry and quantity fields
- Handles post-checkout state to prevent accidental additions

**Permission Properties:**
- `CanRestock`: Whether user can restock
- `CanManageStock`: Whether user can manage stock
- `CanAccessAdminPanel`: Whether user can access admin panel

### AdminViewModel.cs

**What it does:** Manages the admin panel (user management, product management, reports).

**Key Responsibilities:**
- Manage users (add, enable/disable)
- Manage products (add)
- Display reports (sales, orders, top products)
- Manage attendance records
- Calculate monthly attendance summaries

**Important Properties:**
- `Users`: List of users (ObservableCollection)
- `Products`: List of products (ObservableCollection)
- `RecentOrders`: Recent orders (ObservableCollection)
- `TopProducts`: Top selling products (ObservableCollection)
- `TotalSales`, `TotalOrders`, `AverageOrderValue`: Report metrics

**Important Commands:**
- `AddUserCommand`: Creates new user
- `AddProductCommand`: Creates new product
- `ToggleUserStatusCommand`: Enables/disables user
- `OpenAttendancePageCommand`: Opens attendance page

**Key Methods:**
- `InitializeAsync()`: Loads all data
- `AddUserAsync()`: Creates new user
- `AddProductAsync()`: Creates new product
- `ToggleUserStatusAsync()`: Enables/disables user
- `LoadReportsAsync()`: Calculates report metrics

**Permission Properties:**
- `CanManageUsers`: Whether user can manage users
- `CanManageStock`: Whether user can manage stock
- `CanUseAttendanceTracker`: Whether user can use attendance tracker
- `IsDeveloper`: Whether user is developer

### RestockReportViewModel.cs

**What it does:** Manages the restock report page.

**Key Responsibilities:**
- Display restock history
- Filter by product/user
- Localize display values

**Important Properties:**
- `RestockRecords`: List of restock records (ObservableCollection)

**Key Methods:**
- `LoadRestockRecordsAsync()`: Loads all restock records
- `RefreshLocalizedStrings()`: Updates localized display values

**Display Class:**
- `RestockRecordDisplay`: Wraps `RestockRecord` with localized display properties

---

## 7. Views (User Interface)

Views are the UI pages that users interact with. Each view consists of a XAML file (layout) and a code-behind file (C# event handlers).

### LoginPage

**Purpose:** User authentication.

**Key Features:**
- Username and password entry
- Language selector (EN/AR)
- Keyboard navigation (Enter moves focus, triggers login)
- Auto-focus on username field
- Error message display

**XAML File:** `LoginPage.xaml`
- Defines UI layout (labels, entries, buttons)
- Uses data binding for localization

**Code-Behind:** `LoginPage.xaml.cs`
- `OnLoginClicked()`: Handles login button click
- `OnUsernameEntryCompleted()`: Moves focus to password field
- `OnPasswordEntryCompleted()`: Triggers login
- `UpdateLocalizedStrings()`: Updates UI text based on language
- `UpdateRTL()`: Handles RTL layout for Arabic

**Navigation:**
- On successful login: Navigates to `//shop` (MainPage)
- On failure: Shows error message, refocuses username field

### MainPage

**Purpose:** Main shop interface for selling products.

**Key Features:**
- Barcode entry for quick product lookup
- Product grid display
- Shopping cart
- Checkout button
- Restock button (if user has permission)
- Admin panel button (if user has permission)
- Open drawer button
- Quick checkout (F1 keyboard shortcut)

**XAML File:** `MainPage.xaml`
- Defines layout with barcode entry, product grid, cart list
- Uses data binding to ViewModel properties
- CollectionView for products and cart items

**Code-Behind:** `MainPage.xaml.cs`
- `OnBarcodeEntryCompleted()`: Handles barcode entry (moves focus, triggers add)
- `OnQuantityEntryCompleted()`: Handles quantity entry (adds to cart)
- `OnQuantityEntryFocused()`: Selects text for easy replacement
- `UpdateLocalizedStrings()`: Updates UI text
- `SetupKeyboardShortcuts()`: Registers F1 for quick checkout (Windows)

**Keyboard Navigation:**
- Enter in barcode field: Moves to quantity or triggers checkout
- Enter in quantity field: Adds to cart, refocuses barcode
- F1: Quick checkout (if cart has items)

### AdminPage

**Purpose:** Admin panel for managing users, products, and viewing reports.

**Key Features:**
- User management section (if user has permission)
- Product management section
- Reports & Insights section
- Attendance tracker button (if user has permission)
- Restock report button
- Create user button (opens DeveloperSetupPage)

**XAML File:** `AdminPage.xaml`
- Sections for users, products, reports
- CollectionView for displaying lists
- Conditional visibility based on permissions

**Code-Behind:** `AdminPage.xaml.cs`
- `OnCreateUserClicked()`: Navigates to DeveloperSetupPage
- `OnOpenRestockReportClicked()`: Navigates to RestockReportPage
- `UpdateLocalizedStrings()`: Updates UI text
- `UpdateRTL()`: Handles RTL layout

### DeveloperSetupPage

**Purpose:** Form for creating new users (accessible from Admin Panel).

**Key Features:**
- User creation form (Name, Username, Password, Confirm Password, Role, Salary)
- Back button
- Language selector
- Validation
- Role picker (User, Admin, Moderator - no Developer option)

**XAML File:** `DeveloperSetupPage.xaml`
- Form fields with labels
- Role picker
- Create button

**Code-Behind:** `DeveloperSetupPage.xaml.cs`
- `OnCreateUserClicked()`: Validates and creates user
- `OnBackButtonClicked()`: Navigates back
- `UpdateLocalizedStrings()`: Updates UI text
- Keyboard navigation between fields

### AttendancePage

**Purpose:** Employee attendance tracking.

**Key Features:**
- Record attendance for employees
- Monthly attendance calendar
- Attendance summaries
- Check-in/check-out time tracking

**XAML File:** `AttendancePage.xaml`
- Attendance form
- Calendar display
- Summary section

**Code-Behind:** `AttendancePage.xaml.cs`
- Handles attendance recording
- Displays monthly summaries
- Updates localized strings

### RestockReportPage

**Purpose:** Display restock history.

**Key Features:**
- List of all restock records
- Shows who restocked, when, and how much
- Localized display values

**XAML File:** `RestockReportPage.xaml`
- CollectionView for restock records
- Empty view when no records

**Code-Behind:** `RestockReportPage.xaml.cs`
- Loads restock records
- Updates localized strings
- Handles back navigation

### AppShell

**Purpose:** Defines application navigation structure.

**XAML File:** `AppShell.xaml`
- Defines routes for pages
- Sets default route (login)
- Disables flyout menu

**Routes:**
- `//login`: LoginPage
- `//shop`: MainPage
- `developersetup`: DeveloperSetupPage (not direct route, accessed via navigation)

---

## 8. Application Lifecycle

### MauiProgram.cs

**What it does:** Configures the MAUI application and registers services.

**Key Responsibilities:**
- Create MAUI app builder
- Register fonts
- Register services (Singleton, Transient)
- Configure platform-specific settings (Windows window size)

**Service Registration:**
```csharp
.Services.AddSingleton<DatabaseService>()      // One instance for entire app
.AddSingleton<AuthService>()
.AddSingleton<CartService>()
.AddSingleton<LocalizationService>(_ => LocalizationService.Instance)
#if WINDOWS
.AddSingleton<IPrintService, WindowsPrintService>()      // Windows-specific
.AddSingleton<ICashDrawerService, WindowsCashDrawerService>()
#else
.AddSingleton<IPrintService, DefaultPrintService>()     // Default for other platforms
.AddSingleton<ICashDrawerService, DefaultCashDrawerService>()
#endif
.AddSingleton<ShopViewModel>()                  // ViewModels
.AddSingleton<AdminViewModel>()
.AddTransient<LoginPage>()                      // New instance each time
.AddTransient<AdminPage>()
```

**Service Lifetimes:**
- **Singleton**: One instance for entire app lifetime (services, ViewModels)
- **Transient**: New instance each time requested (pages)

**Platform-Specific Code:**
- Windows: Maximizes window on startup
- Other platforms: Default behavior

### App.xaml.cs

**What it does:** Application entry point, handles app-level events.

**Key Responsibilities:**
- Initialize app
- Navigate to login page on startup
- Handle language changes
- Set RTL layout based on language

**Startup Flow:**
1. App constructor runs
2. Creates AppShell
3. Navigates to `//login` route
4. Initializes localization
5. Sets RTL if Arabic is selected

### AppShell.xaml.cs

**What it does:** Shell navigation setup.

**Key Responsibilities:**
- Initialize Shell component
- Handle navigation events (if needed)

**Navigation:**
- Uses MAUI Shell navigation
- Routes defined in `AppShell.xaml`
- Navigation via `Shell.Current.GoToAsync("//route")`

---

## 9. Platform-Specific Code

### Windows Platform

**Location:** `Platforms/Windows/`

**WindowsPrintService.cs:**
- Creates temporary HTML file with receipt content
- Opens in default browser
- Uses JavaScript to auto-trigger print dialog
- Cleans up temporary file after delay

**WindowsCashDrawerService.cs:**
- Sends ESC/POS command to printer
- Methods:
  1. Direct serial/parallel port communication
  2. Raw print command via Windows `copy` command
- Uses `System.Management` to query printer information
- Uses `System.IO.Ports` for serial communication

**App.xaml.cs (Windows):**
- Platform-specific app initialization (if needed)

### Other Platforms

**Default Implementations:**
- `DefaultPrintService`: Uses MAUI Share API
- `DefaultCashDrawerService`: Returns false (not supported)

**Platform Folders:**
- `Platforms/Android/`: Android-specific code
- `Platforms/iOS/`: iOS-specific code
- `Platforms/MacCatalyst/`: macOS-specific code

---

## 10. Utilities & Helpers

### PasswordHelper.cs

**What it does:** Provides secure password hashing and verification.

**Methods:**
- `HashPassword(password)`: Hashes password using PBKDF2
- `VerifyPassword(password, hashedPassword)`: Verifies password against hash

**Algorithm:**
- Uses PBKDF2 (Password-Based Key Derivation Function 2)
- SHA-256 hash algorithm
- 10,000 iterations
- 16-byte salt
- 32-byte hash
- Stores as `salt:hash` (Base64 encoded)

**Security:**
- Salt prevents rainbow table attacks
- High iteration count slows brute force attacks
- Fixed-time comparison prevents timing attacks

**Example:**
```csharp
// Hash password
string hashed = PasswordHelper.HashPassword("mypassword");
// Result: "base64salt:base64hash"

// Verify password
bool isValid = PasswordHelper.VerifyPassword("mypassword", hashed);
```

### LocalizedStringExtension.cs

**What it does:** XAML markup extension for localized strings.

**Usage in XAML:**
```xml
<Label Text="{local:LocalizedString Key=Login}" />
```

**How it works:**
- Implements `IMarkupExtension`
- Gets current culture from `LocalizationService`
- Loads string from resource file
- Returns localized value or `[Key]` if not found

**Benefits:**
- Easy to use in XAML
- Automatically updates when language changes
- No code-behind needed for simple text

---

## 11. Resources

### Strings.resx & Strings.ar.resx

**What they are:** Resource files containing localized strings.

**Strings.resx:** English strings
**Strings.ar.resx:** Arabic strings

**Structure:**
- Key-value pairs
- Key: Identifier (e.g., "Login")
- Value: Localized text (e.g., "Login" in English, "تسجيل الدخول" in Arabic)

**Usage:**
```csharp
// In C# code
string text = _localizationService.GetString("Login");

// In XAML
<Label Text="{local:LocalizedString Key=Login}" />
```

**Common Keys:**
- `Login`, `Password`, `Username`
- `AddToCart`, `Checkout`, `Total`
- `AdminPanel`, `ReportsInsights`
- `OpenDrawer`, `PrintReceipt`
- And many more...

**Newline Handling:**
- Use `&#10;` in resource files for newlines
- Replaced with `Environment.NewLine` in code

### Styles and Colors

**Location:** `Resources/Styles/` (if exists)

**Purpose:** Define app-wide styles and colors.

**Usage:**
- Consistent theming across app
- Easy to change colors/styles globally

---

## 12. Common Patterns & Best Practices

### Thread Safety (MainThread Usage)

**Problem:** UI updates must happen on the main/UI thread.

**Solution:** Use `MainThread.InvokeOnMainThreadAsync` or `MainThread.BeginInvokeOnMainThread`.

**When to Use:**
- Updating `ObservableCollection` from background thread
- Updating UI properties from async operations
- Any UI-related operation from non-UI thread

**Example:**
```csharp
// Wrong (can cause AccessViolationException)
await Task.Run(() =>
{
    Products.Clear(); // ❌ Not on UI thread
});

// Correct
await MainThread.InvokeOnMainThreadAsync(() =>
{
    Products.Clear(); // ✅ On UI thread
});
```

**In this app:**
- `ShopViewModel.InitializeAsync()`: Uses `MainThread` for collection updates
- `ShopViewModel.UpdateCart()`: Uses `MainThread` for collection updates
- `ShopViewModel.FilterProducts()`: Uses `MainThread` for collection updates

### Error Handling

**Best Practices:**
- Use try-catch blocks for operations that can fail
- Show user-friendly error messages
- Log errors for debugging
- Handle null values gracefully

**Example:**
```csharp
try
{
    await _databaseService.AddProductAsync(product);
    // Show success message
}
catch (Exception ex)
{
    // Log error
    Debug.WriteLine($"Error: {ex.Message}");
    // Show user-friendly message
    await DisplayAlert("Error", "Failed to add product", "OK");
}
```

### Async/Await Patterns

**Best Practices:**
- Use `async`/`await` for I/O operations (database, network)
- Don't block UI thread with synchronous operations
- Use `ConfigureAwait(false)` in library code (not needed in MAUI)

**Example:**
```csharp
// Good
public async Task LoadProductsAsync()
{
    var products = await _databaseService.GetProductsAsync();
    // Update UI
}

// Bad (blocks UI thread)
public void LoadProducts()
{
    var products = _databaseService.GetProductsAsync().Result; // ❌ Blocks
}
```

### Property Change Notifications

**Pattern:**
```csharp
private string _name;
public string Name
{
    get => _name;
    set
    {
        if (_name != value) // Only update if changed
        {
            _name = value;
            OnPropertyChanged(); // Notify UI
        }
    }
}
```

**Why:**
- Prevents unnecessary UI updates
- Ensures UI stays in sync with data
- Required for data binding to work

### Command Patterns

**Pattern:**
```csharp
public ICommand MyCommand { get; }

public MyViewModel()
{
    MyCommand = new Command(
        execute: async () => await DoSomethingAsync(),
        canExecute: () => !IsBusy && HasData
    );
}

private void UpdateCommandCanExecute()
{
    (MyCommand as Command)?.ChangeCanExecute();
}
```

**Benefits:**
- Can enable/disable based on conditions
- Separates UI from business logic
- Reusable across different UI elements

### Focus Management

**Pattern:**
```csharp
// Move focus programmatically
if (EntryField != null)
{
    EntryField.Focus();
}

// Unfocus
if (EntryField != null && EntryField.IsFocused)
{
    EntryField.Unfocus();
}
```

**In this app:**
- Barcode entry → Quantity entry (normal flow)
- After checkout/drawer → Barcode entry (reset flow)
- Keyboard navigation (Enter key handling)

### Localization Best Practices

**Pattern:**
```csharp
// In code-behind
private void UpdateLocalizedStrings()
{
    if (MyLabel != null)
    {
        MyLabel.Text = _localizationService.GetString("MyKey");
    }
}

// In XAML
<Label Text="{local:LocalizedString Key=MyKey}" />
```

**Best Practices:**
- Always use localization service, never hardcode strings
- Update strings when language changes
- Handle RTL layout for Arabic
- Use `&#10;` for newlines in resource files

### Database Best Practices

**Pattern:**
```csharp
// Always initialize before use
await InitializeAsync();

// Use async methods
var products = await _databaseService.GetProductsAsync();

// Handle errors
try
{
    await _databaseService.SaveAsync(item);
}
catch (Exception ex)
{
    // Handle error
}
```

**In this app:**
- Lazy initialization (only when needed)
- Thread-safe initialization (SemaphoreSlim)
- WAL mode for better concurrency
- Error handling for missing tables

---

## Key Takeaways

1. **MVVM Pattern**: Separates UI from business logic, makes code testable and maintainable.

2. **Dependency Injection**: Services are injected into ViewModels, making code flexible and testable.

3. **Data Binding**: UI automatically updates when data changes (ObservableCollection, INotifyPropertyChanged).

4. **Async/Await**: Use for all I/O operations to keep UI responsive.

5. **Thread Safety**: Always update UI collections on the main thread.

6. **Localization**: Use resource files and LocalizationService for multi-language support.

7. **Platform-Specific Code**: Use `#if WINDOWS` for platform-specific implementations.

8. **Commands**: Use ICommand for handling user actions in MVVM.

9. **Error Handling**: Always handle errors gracefully and show user-friendly messages.

10. **Focus Management**: Programmatically manage focus for better UX.

---

## Common Questions

### Q: How does data binding work?

**A:** Data binding connects UI elements to ViewModel properties. When a property changes and calls `OnPropertyChanged()`, the UI automatically updates. For collections, use `ObservableCollection` which notifies the UI when items are added/removed.

### Q: Why use Dependency Injection?

**A:** DI makes code more testable and flexible. Instead of creating dependencies inside a class, they're provided from outside. This allows you to easily swap implementations (e.g., mock services for testing).

### Q: How does localization work?

**A:** Strings are stored in `.resx` resource files (one per language). `LocalizationService` loads the appropriate file based on current culture. When language changes, the service fires an event, and views update their strings.

### Q: Why use async/await?

**A:** Async operations (like database queries) can take time. Without async, the UI would freeze. With async/await, the UI stays responsive while waiting for the operation to complete.

### Q: How does the cart work?

**A:** `CartService` manages cart items in memory and persists them to the database. When items are added/removed, the service fires `OnCartChanged` event. ViewModels listen to this event and update the UI.

### Q: How are permissions checked?

**A:** The `User` model has permission properties (e.g., `CanRestock`). `AuthService` exposes these properties. ViewModels check these properties to enable/disable features. Views use data binding to show/hide UI elements.

### Q: How does barcode scanning work?

**A:** User types barcode in `ProductSearchEntry`. `ShopViewModel` filters products based on barcode. When a product is found, it's selected. User enters quantity and presses Enter, which adds the product to cart.

### Q: How does receipt printing work on Windows?

**A:** `WindowsPrintService` creates a temporary HTML file with the receipt content. It opens the file in the default browser, which auto-triggers the print dialog via JavaScript. After printing, the temporary file is deleted.

### Q: How does cash drawer opening work?

**A:** `WindowsCashDrawerService` sends ESC/POS commands to the printer. It tries multiple methods: direct serial/parallel port communication, or raw print command. The printer interprets the command and opens the drawer.

### Q: What is the difference between Singleton and Transient?

**A:** **Singleton**: One instance for the entire app lifetime. Used for services and ViewModels. **Transient**: New instance each time requested. Used for pages that should be fresh each time.

---

## Conclusion

This documentation provides a comprehensive overview of the SweetShopMa application. The codebase follows MVVM architecture, uses dependency injection, and implements best practices for cross-platform development.

**Next Steps:**
1. Read the code comments in each file for detailed explanations
2. Experiment with the code to understand how it works
3. Add new features following the existing patterns
4. Refer to this documentation when you need to understand a concept

**Happy Coding! 🎉**

