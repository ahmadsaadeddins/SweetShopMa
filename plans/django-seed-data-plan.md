# Django Seed Data Plan - C# to Django Migration

## Overview
Create mock seed data from the C# MAUI project structure to populate the Django backend for testing and development.

## Data Mapping: C# → Django

### 1. Categories (C# Product.Category → Django Category)
| C# Property | Django Field | Notes |
|-------------|--------------|-------|
| Category (string) | name | Category name from Product |
| - | description | Auto-generated or empty |
| - | created_at | Auto by Django |
| - | updated_at | Auto by Django |

### 2. Products (C# Product → Django Product)
| C# Property | Django Field | Notes |
|-------------|--------------|-------|
| Id | - | Not mapped (auto-generated) |
| Name | name | Product name |
| Emoji | description | Store emoji in description |
| Barcode | barcode | Product barcode |
| Price | price | Selling price |
| Stock | quantity | Current stock |
| IsSoldByWeight | unit | "kg" if true, "piece" if false |
| Category | category | FK to Category |
| - | cost | Set to 70% of price (mock) |
| - | low_stock_threshold | Default 10 |
| - | sku | Auto-generated from name |
| - | is_active | Default true |

### 3. Users (C# User → Django User + Staff)
| C# Property | Django Field | Notes |
|-------------|--------------|-------|
| Username | username | Login username |
| Name | first_name + last_name | Split or use as first_name |
| Password | password | Hashed password |
| Role | is_staff + is_superuser | Admin→superuser, Moderator→staff |
| MonthlySalary | - | Not in Django User (store in profile if needed) |
| IsEnabled | is_active | Active status |

### 4. Sales (C# Order → Django Sale)
| C# Property | Django Field | Notes |
|-------------|--------------|-------|
| Id | - | Not mapped (auto-generated) |
| UserId | staff | FK to User |
| UserName | - | Denormalized, get from staff |
| OrderDate | created_at | Order timestamp |
| Total | total | Order total |
| ItemCount | - | Computed from items |
| Status | status | Order status |
| - | subtotal | 90% of total (mock) |
| - | tax | 5% of subtotal (mock) |
| - | discount | 5% of subtotal (mock) |
| - | payment_method | Random from choices |
| - | customer | Null (no customer in C#) |
| - | synced | False (new data) |

### 5. SaleItems (C# OrderItem → Django SaleItem)
| C# Property | Django Field | Notes |
|-------------|--------------|-------|
| Id | - | Not mapped (auto-generated) |
| OrderId | sale | FK to Sale |
| ProductId | product | FK to Product |
| Name | - | Get from product |
| Emoji | - | Not stored |
| Price | unit_price | Price at time of sale |
| Quantity | quantity | Quantity sold |
| IsSoldByWeight | - | Not stored |
| - | cost_price | 70% of unit_price (mock) |
| - | subtotal | unit_price × quantity |
| - | discount | 0 (no discount per item) |

---

## Implementation Plan

### Step 1: Create Mock Data Generator Script
**File:** `sweetshopma-desktop/backend/generate_seed_data.py`

Generate Python dictionaries with mock data based on C# model structure.

### Step 2: Create Django Management Command
**File:** `sweetshopma-desktop/backend/api/management/commands/seed_data.py`

Django command to load seed data into database.

### Step 3: Create Data Migration (Alternative)
**File:** `sweetshopma-desktop/backend/api/migrations/0002_seed_initial_data.py`

Django migration that runs on migrate command.

### Step 4: Create Fixture Files (Alternative)
**Files:**
- `sweetshopma-desktop/backend/fixtures/categories.json`
- `sweetshopma-desktop/backend/fixtures/products.json`
- `sweetshopma-desktop/backend/fixtures/users.json`
- `sweetshopma-desktop/backend/fixtures/sales.json`

---

## Mock Data Structure

### Categories (5 items)
```python
categories = [
    {"name": "Candy", "description": "Various candies and sweets"},
    {"name": "Chocolate", "description": "Chocolate bars and treats"},
    {"name": "Cakes", "description": "Cakes and pastries"},
    {"name": "Drinks", "description": "Beverages"},
    {"name": "Ice Cream", "description": "Frozen desserts"},
]
```

### Products (20 items)
```python
products = [
    {"name": "Chocolate Bar", "emoji": "🍫", "category": "Chocolate", "price": 5.50, "stock": 100, "is_weight": False},
    {"name": "Gummy Bears", "emoji": "🍬", "category": "Candy", "price": 3.00, "stock": 50.5, "is_weight": True},
    {"name": "Vanilla Cake", "emoji": "🍰", "category": "Cakes", "price": 45.00, "stock": 10, "is_weight": False},
    # ... more products
]
```

### Users (5 items)
```python
users = [
    {"username": "admin", "name": "System Admin", "role": "Admin", "password": "admin123"},
    {"username": "cashier1", "name": "Ahmed Mohamed", "role": "User", "password": "cashier123"},
    {"username": "moderator1", "name": "Sara Ali", "role": "Moderator", "password": "mod123"},
    # ... more users
]
```

### Sales (10 items with SaleItems)
```python
sales = [
    {
        "user": "cashier1",
        "date": "2024-01-15 10:30:00",
        "total": 25.50,
        "items": [
            {"product": "Chocolate Bar", "quantity": 2, "price": 5.50},
            {"product": "Gummy Bears", "quantity": 1.5, "price": 3.00},
        ]
    },
    # ... more sales
]
```

---

## Usage Commands

### Option 1: Management Command
```bash
cd sweetshopma-desktop/backend
python manage.py seed_data
```

### Option 2: Load Fixtures
```bash
cd sweetshopma-desktop/backend
python manage.py loaddata fixtures/categories.json
python manage.py loaddata fixtures/products.json
python manage.py loaddata fixtures/users.json
python manage.py loaddata fixtures/sales.json
```

### Option 3: Run Migration
```bash
cd sweetshopma-desktop/backend
python manage.py migrate
```

---

## File Structure
```
sweetshopma-desktop/
├── backend/
│   ├── api/
│   │   ├── management/
│   │   │   └── commands/
│   │   │       └── seed_data.py          # Django management command
│   │   └── migrations/
│   │       └── 0002_seed_initial_data.py # Data migration
│   ├── fixtures/
│   │   ├── categories.json               # Category fixtures
│   │   ├── products.json                 # Product fixtures
│   │   ├── users.json                    # User fixtures
│   │   └── sales.json                    # Sale fixtures
│   └── generate_seed_data.py             # Mock data generator
```

---

## Notes
- Passwords will be hashed using Django's `make_password()`
- Product costs will be set to 70% of price (mock profit margin)
- SKUs will be auto-generated from product names
- Sale dates will be within the last 30 days
- All data will be marked as `synced=False` for testing sync functionality
