# SweetShopMa Backend (Django)

Django REST API backend for the SweetShopMa Point of Sale system.

## Setup

1. **Create virtual environment:**
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

2. **Install dependencies:**
```bash
pip install -r requirements.txt
```

3. **Run migrations:**
```bash
python manage.py migrate
```

4. **Seed initial data:**
```bash
python manage.py seed_data
```

This creates:
- Default Developer user: `ama` / `AsrAma12@#`
- Sample products

5. **Create superuser (optional, for Django admin):**
```bash
python manage.py createsuperuser
```

6. **Run development server:**
```bash
python manage.py runserver
```

The API will be available at `http://localhost:8000/api/`

## API Endpoints

### Authentication
- `POST /api/auth/login/` - Login (returns JWT tokens)
- `POST /api/auth/refresh/` - Refresh access token

### Products
- `GET /api/products/` - List products
- `GET /api/products/{id}/` - Get product details
- `POST /api/products/` - Create product (Admin/Moderator)
- `PUT /api/products/{id}/` - Update product
- `DELETE /api/products/{id}/` - Delete product
- `POST /api/products/{id}/restock/` - Restock product

### Cart
- `GET /api/cart/` - Get cart items
- `POST /api/cart/` - Add item to cart
- `PUT /api/cart/{id}/` - Update cart item
- `DELETE /api/cart/{id}/` - Remove item from cart
- `GET /api/cart/total/` - Get cart total

### Orders
- `GET /api/orders/` - List orders
- `GET /api/orders/{id}/` - Get order details
- `POST /api/orders/checkout/` - Checkout (create order from cart)

### Users
- `GET /api/users/` - List users (Developer/Admin only)
- `POST /api/users/` - Create user
- `PUT /api/users/{id}/` - Update user
- `POST /api/users/{id}/toggle_status/` - Enable/disable user

### Attendance
- `GET /api/attendance/` - List attendance records
- `POST /api/attendance/` - Create attendance record
- `GET /api/attendance/monthly_summary/` - Get monthly summary

### Reports
- `GET /api/reports/` - Get sales reports and analytics

## Authentication

The API uses JWT (JSON Web Tokens) for authentication. Include the access token in the Authorization header:

```
Authorization: Bearer <access_token>
```

## User Roles

- **Developer**: Full access, can create users
- **Admin**: Can manage users, products, attendance, restock
- **Moderator**: Can manage stock, attendance, restock (but NOT users)
- **User**: Can only sell (use shop interface)

## Database

Uses SQLite by default (development). For production, configure PostgreSQL in `settings.py`.

