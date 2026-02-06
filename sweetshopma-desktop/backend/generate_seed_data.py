"""
Seed Data Generator for SweetShopMa Django Backend.

This module generates mock data based on the C# MAUI project structure.
It creates Categories, Products, Users, Sales, and SaleItems for testing.

Based on C# Models:
- Product (SweetShopMa/Models/Product.cs)
- User (SweetShopMa/Models/User.cs)
- Order (SweetShopMa/Models/Order.cs)
- OrderItem (SweetShopMa/Models/OrderItem.cs)
"""

import random
from datetime import datetime, timedelta
from decimal import Decimal


# ============================================================================
# MOCK DATA CONFIGURATION
# ============================================================================

# Categories based on C# Product.Category
CATEGORIES = [
    {"name": "Candy", "description": "Various candies and sweets 🍬"},
    {"name": "Chocolate", "description": "Chocolate bars and treats 🍫"},
    {"name": "Cakes", "description": "Cakes and pastries 🍰"},
    {"name": "Drinks", "description": "Beverages and refreshments 🥤"},
    {"name": "Ice Cream", "description": "Frozen desserts and ice cream 🍨"},
]

# Products based on C# Product model
PRODUCTS = [
    # Candy Category
    {"name": "Gummy Bears", "emoji": "🍬", "category": "Candy", "price": 3.00, "stock": 50.5, "is_weight": True, "barcode": "1001"},
    {"name": "Lollipops", "emoji": "🍭", "category": "Candy", "price": 2.50, "stock": 200, "is_weight": False, "barcode": "1002"},
    {"name": "Hard Candy Mix", "emoji": "🍬", "category": "Candy", "price": 4.00, "stock": 35.0, "is_weight": True, "barcode": "1003"},
    {"name": "Cotton Candy", "emoji": "🍬", "category": "Candy", "price": 5.00, "stock": 50, "is_weight": False, "barcode": "1004"},
    {"name": "Jelly Beans", "emoji": "🍬", "category": "Candy", "price": 3.50, "stock": 40.0, "is_weight": True, "barcode": "1005"},

    # Chocolate Category
    {"name": "Chocolate Bar", "emoji": "🍫", "category": "Chocolate", "price": 5.50, "stock": 100, "is_weight": False, "barcode": "2001"},
    {"name": "Dark Chocolate", "emoji": "🍫", "category": "Chocolate", "price": 7.00, "stock": 80, "is_weight": False, "barcode": "2002"},
    {"name": "White Chocolate", "emoji": "🍫", "category": "Chocolate", "price": 6.50, "stock": 75, "is_weight": False, "barcode": "2003"},
    {"name": "Chocolate Truffles", "emoji": "🍫", "category": "Chocolate", "price": 12.00, "stock": 30.0, "is_weight": True, "barcode": "2004"},
    {"name": "Chocolate Covered Nuts", "emoji": "🍫", "category": "Chocolate", "price": 8.00, "stock": 25.0, "is_weight": True, "barcode": "2005"},

    # Cakes Category
    {"name": "Vanilla Cake", "emoji": "🍰", "category": "Cakes", "price": 45.00, "stock": 10, "is_weight": False, "barcode": "3001"},
    {"name": "Chocolate Cake", "emoji": "🍰", "category": "Cakes", "price": 50.00, "stock": 8, "is_weight": False, "barcode": "3002"},
    {"name": "Cheesecake", "emoji": "🍰", "category": "Cakes", "price": 55.00, "stock": 6, "is_weight": False, "barcode": "3003"},
    {"name": "Cupcakes (6 pack)", "emoji": "🧁", "category": "Cakes", "price": 18.00, "stock": 20, "is_weight": False, "barcode": "3004"},
    {"name": "Brownies", "emoji": "🍫", "category": "Cakes", "price": 15.00, "stock": 25.0, "is_weight": True, "barcode": "3005"},

    # Drinks Category
    {"name": "Orange Juice", "emoji": "🥤", "category": "Drinks", "price": 8.00, "stock": 40, "is_weight": False, "barcode": "4001"},
    {"name": "Apple Juice", "emoji": "🥤", "category": "Drinks", "price": 8.00, "stock": 35, "is_weight": False, "barcode": "4002"},
    {"name": "Milkshake", "emoji": "🥤", "category": "Drinks", "price": 12.00, "stock": 50, "is_weight": False, "barcode": "4003"},
    {"name": "Iced Coffee", "emoji": "☕", "category": "Drinks", "price": 10.00, "stock": 60, "is_weight": False, "barcode": "4004"},
    {"name": "Sparkling Water", "emoji": "🥤", "category": "Drinks", "price": 5.00, "stock": 100, "is_weight": False, "barcode": "4005"},

    # Ice Cream Category
    {"name": "Vanilla Ice Cream", "emoji": "🍨", "category": "Ice Cream", "price": 15.00, "stock": 30.0, "is_weight": True, "barcode": "5001"},
    {"name": "Chocolate Ice Cream", "emoji": "🍨", "category": "Ice Cream", "price": 15.00, "stock": 25.0, "is_weight": True, "barcode": "5002"},
    {"name": "Strawberry Ice Cream", "emoji": "🍨", "category": "Ice Cream", "price": 15.00, "stock": 20.0, "is_weight": True, "barcode": "5003"},
    {"name": "Ice Cream Cone", "emoji": "🍦", "category": "Ice Cream", "price": 8.00, "stock": 80, "is_weight": False, "barcode": "5004"},
    {"name": "Ice Cream Sundae", "emoji": "🍨", "category": "Ice Cream", "price": 18.00, "stock": 40, "is_weight": False, "barcode": "5005"},
]

# Users based on C# User model with Role constants
USERS = [
    {
        "username": "admin",
        "name": "System Administrator",
        "role": "Developer",  # Full access
        "password": "admin123",
        "monthly_salary": 15000.00,
        "overtime_multiplier": 1.5,
    },
    {
        "username": "manager",
        "name": "Ahmed Hassan",
        "role": "Admin",  # Can manage users, products, attendance
        "password": "manager123",
        "monthly_salary": 8000.00,
        "overtime_multiplier": 1.5,
    },
    {
        "username": "moderator1",
        "name": "Sara Ali",
        "role": "Moderator",  # Can manage stock, attendance, restock
        "password": "mod123",
        "monthly_salary": 6000.00,
        "overtime_multiplier": 1.5,
    },
    {
        "username": "cashier1",
        "name": "Mohamed Ibrahim",
        "role": "User",  # Can only sell
        "password": "cashier123",
        "monthly_salary": 4000.00,
        "overtime_multiplier": 1.5,
    },
    {
        "username": "cashier2",
        "name": "Fatima Mahmoud",
        "role": "User",  # Can only sell
        "password": "cashier456",
        "monthly_salary": 4000.00,
        "overtime_multiplier": 1.5,
    },
]

# Sample Sales based on C# Order model
SALES = [
    {
        "username": "cashier1",
        "date": "2024-01-15 10:30:00",
        "total": 25.50,
        "items": [
            {"product": "Chocolate Bar", "quantity": 2, "price": 5.50},
            {"product": "Gummy Bears", "quantity": 1.5, "price": 3.00},
            {"product": "Orange Juice", "quantity": 1, "price": 8.00},
        ],
    },
    {
        "username": "cashier1",
        "date": "2024-01-15 14:20:00",
        "total": 73.00,
        "items": [
            {"product": "Vanilla Cake", "quantity": 1, "price": 45.00},
            {"product": "Milkshake", "quantity": 2, "price": 12.00},
            {"product": "Lollipops", "quantity": 4, "price": 2.50},
        ],
    },
    {
        "username": "cashier2",
        "date": "2024-01-16 09:15:00",
        "total": 38.00,
        "items": [
            {"product": "Cheesecake", "quantity": 1, "price": 55.00, "discount": 20.00},
        ],
    },
    {
        "username": "cashier2",
        "date": "2024-01-16 16:45:00",
        "total": 19.50,
        "items": [
            {"product": "Dark Chocolate", "quantity": 2, "price": 7.00},
            {"product": "Ice Cream Cone", "quantity": 1, "price": 8.00},
        ],
    },
    {
        "username": "cashier1",
        "date": "2024-01-17 11:00:00",
        "total": 42.00,
        "items": [
            {"product": "Chocolate Covered Nuts", "quantity": 2.0, "price": 8.00},
            {"product": "Iced Coffee", "quantity": 2, "price": 10.00},
            {"product": "Cupcakes (6 pack)", "quantity": 1, "price": 18.00},
        ],
    },
    {
        "username": "cashier2",
        "date": "2024-01-17 15:30:00",
        "total": 31.00,
        "items": [
            {"product": "Chocolate Truffles", "quantity": 1.5, "price": 12.00},
            {"product": "Apple Juice", "quantity": 1, "price": 8.00},
            {"product": "Brownies", "quantity": 0.5, "price": 15.00},
        ],
    },
    {
        "username": "cashier1",
        "date": "2024-01-18 10:00:00",
        "total": 55.00,
        "items": [
            {"product": "Chocolate Cake", "quantity": 1, "price": 50.00},
            {"product": "Sparkling Water", "quantity": 1, "price": 5.00},
        ],
    },
    {
        "username": "cashier2",
        "date": "2024-01-18 13:20:00",
        "total": 28.50,
        "items": [
            {"product": "Vanilla Ice Cream", "quantity": 1.0, "price": 15.00},
            {"product": "Strawberry Ice Cream", "quantity": 0.5, "price": 15.00},
            {"product": "Hard Candy Mix", "quantity": 0.5, "price": 4.00},
        ],
    },
    {
        "username": "cashier1",
        "date": "2024-01-19 09:45:00",
        "total": 34.00,
        "items": [
            {"product": "White Chocolate", "quantity": 3, "price": 6.50},
            {"product": "Cotton Candy", "quantity": 2, "price": 5.00},
            {"product": "Jelly Beans", "quantity": 0.5, "price": 3.50},
        ],
    },
    {
        "username": "cashier2",
        "date": "2024-01-19 17:00:00",
        "total": 48.00,
        "items": [
            {"product": "Ice Cream Sundae", "quantity": 2, "price": 18.00},
            {"product": "Chocolate Ice Cream", "quantity": 0.5, "price": 15.00},
            {"product": "Lollipops", "quantity": 2, "price": 2.50},
        ],
    },
]


# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

def generate_sku(product_name: str) -> str:
    """Generate SKU from product name."""
    # Take first 3 letters of each word, uppercase, remove spaces
    words = product_name.split()
    sku = ''.join([word[:3].upper() for word in words if len(word) >= 3])
    return sku[:8] if len(sku) > 8 else sku


def calculate_cost(price: Decimal) -> Decimal:
    """Calculate cost as 70% of price (mock profit margin)."""
    return Decimal(str(price)) * Decimal('0.70')


def generate_random_date(days_back: int = 30) -> datetime:
    """Generate a random datetime within the last N days."""
    now = datetime.now()
    random_days = random.randint(0, days_back)
    random_hours = random.randint(8, 20)  # Business hours
    random_minutes = random.randint(0, 59)
    
    return now - timedelta(
        days=random_days,
        hours=(24 - random_hours),
        minutes=random_minutes
    )


# ============================================================================
# DATA GENERATORS
# ============================================================================

def get_categories_data():
    """Return categories data for seeding."""
    return CATEGORIES


def get_products_data():
    """Return products data for seeding."""
    products = []
    for p in PRODUCTS:
        price = Decimal(str(p["price"]))
        cost = calculate_cost(p["price"])
        products.append({
            "name": p["name"],
            "description": f"{p['emoji']} {p['name']}",
            "category_name": p["category"],
            "price": float(price),  # Convert to float for SQLite compatibility
            "cost": float(cost),    # Convert to float for SQLite compatibility
            "quantity": int(p["stock"]) if not p["is_weight"] else 0,
            "unit": "kg" if p["is_weight"] else "piece",
            "low_stock_threshold": 10,
            "barcode": p["barcode"],
            "sku": generate_sku(p["name"]),
            "is_active": True,
        })
    return products


def get_users_data():
    """Return users data for seeding."""
    users = []
    for u in USERS:
        # Determine Django permissions based on C# role
        is_superuser = u["role"] == "Developer"
        is_staff = u["role"] in ["Developer", "Admin", "Moderator"]
        
        users.append({
            "username": u["username"],
            "password": u["password"],  # Will be hashed in management command
            "first_name": u["name"].split()[0] if " " in u["name"] else u["name"],
            "last_name": u["name"].split()[1] if " " in u["name"] else "",
            "email": f"{u['username']}@sweetshop.local",
            "is_staff": is_staff,
            "is_superuser": is_superuser,
            "is_active": True,
            # Additional fields for future profile extension
            "role": u["role"],
            "monthly_salary": float(u["monthly_salary"]),  # Convert to float
            "overtime_multiplier": float(u["overtime_multiplier"]),  # Convert to float
        })
    return users


def get_sales_data():
    """Return sales data for seeding."""
    sales = []
    for s in SALES:
        # Calculate subtotal, tax, discount
        total = Decimal(str(s["total"]))
        subtotal = total * Decimal('0.95')  # 95% of total
        tax = subtotal * Decimal('0.05')  # 5% tax
        discount = total - subtotal - tax  # Remaining difference
        
        sales.append({
            "username": s["username"],
            "created_at": datetime.strptime(s["date"], "%Y-%m-%d %H:%M:%S"),
            "total": float(total),      # Convert to float for SQLite
            "subtotal": float(subtotal), # Convert to float for SQLite
            "tax": float(tax),           # Convert to float for SQLite
            "discount": float(discount), # Convert to float for SQLite
            "payment_method": random.choice(["cash", "card", "mobile"]),
            "status": "completed",
            "items": s["items"],
        })
    return sales


# ============================================================================
# MAIN EXPORTS
# ============================================================================

__all__ = [
    "get_categories_data",
    "get_products_data",
    "get_users_data",
    "get_sales_data",
    "generate_sku",
    "calculate_cost",
    "generate_random_date",
]


if __name__ == "__main__":
    # Set UTF-8 encoding for Windows console
    import sys
    if sys.platform == "win32":
        import io
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    
    # Test data generation
    print("=== Categories ===")
    for cat in get_categories_data():
        print(f"  {cat}")
    
    print("\n=== Products ===")
    for prod in get_products_data()[:3]:
        print(f"  {prod}")
    
    print("\n=== Users ===")
    for user in get_users_data():
        print(f"  {user['username']}: role={user['role']}, is_staff={user['is_staff']}")
    
    print("\n=== Sales ===")
    for sale in get_sales_data()[:2]:
        print(f"  {sale['username']}: {sale['total']} EGP at {sale['created_at']}")
