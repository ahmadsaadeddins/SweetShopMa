# SweetShopMa Web Application - Setup Guide

## Quick Start

### Prerequisites
- Python 3.10+ (for Django backend)
- Node.js 18+ (for Next.js frontend)

### Backend Setup

1. **Navigate to backend directory:**
```bash
cd web-app/backend
```

2. **Create and activate virtual environment:**
```bash
# Windows
python -m venv venv
venv\Scripts\activate

# Linux/Mac
python3 -m venv venv
source venv/bin/activate
```

3. **Install dependencies:**
```bash
pip install -r requirements.txt
```

4. **Run migrations:**
```bash
python manage.py migrate
```

5. **Seed initial data:**
```bash
python manage.py seed_data
```

This creates:
- Default Developer user: `ama` / `AsrAma12@#`
- 12 sample products

6. **Start Django server:**
```bash
python manage.py runserver
```

Backend will run on `http://localhost:8000`

### Frontend Setup

1. **Navigate to frontend directory:**
```bash
cd web-app/frontend
```

2. **Install dependencies:**
```bash
npm install
```

3. **Create environment file:**
Create `.env.local`:
```
NEXT_PUBLIC_API_URL=http://localhost:8000/api
```

4. **Start Next.js dev server:**
```bash
npm run dev
```

Frontend will run on `http://localhost:3000`

## Default Login

- **Username:** `ama`
- **Password:** `AsrAma12@#`
- **Role:** Developer (full access)

## Features Implemented

### Backend (Django)
- ✅ Custom User model with role-based permissions
- ✅ Product management (unit-based and weight-based)
- ✅ Shopping cart system
- ✅ Order management
- ✅ Attendance tracking
- ✅ Restock tracking
- ✅ Reports and analytics
- ✅ JWT authentication
- ✅ REST API endpoints

### Frontend (Next.js)
- ✅ User authentication (login/logout)
- ✅ Shop interface (product browsing, barcode search, cart, checkout)
- ✅ Admin panel (reports, user management, product management)
- ✅ Multi-language support (English/Arabic with RTL)
- ✅ Responsive design

## API Endpoints

- `POST /api/auth/login/` - Login
- `GET /api/auth/login/` - Get current user
- `GET /api/products/` - List products
- `POST /api/products/{id}/restock/` - Restock product
- `GET /api/cart/` - Get cart items
- `POST /api/cart/` - Add to cart
- `POST /api/orders/checkout/` - Checkout
- `GET /api/reports/` - Get sales reports
- And more...

See `backend/README.md` for complete API documentation.

## Next Steps

To extend the application:
1. Add more localization strings in `frontend/lib/localization-context.tsx`
2. Implement receipt printing (use browser print dialog)
3. Add attendance tracking UI
4. Add restock report page
5. Enhance admin panel with more features

## Troubleshooting

**Backend won't start:**
- Make sure virtual environment is activated
- Check that all dependencies are installed
- Run `python manage.py migrate` if database errors occur

**Frontend can't connect to backend:**
- Check that Django server is running on port 8000
- Verify `NEXT_PUBLIC_API_URL` in `.env.local`
- Check CORS settings in `backend/sweetshop/settings.py`

**Login fails:**
- Make sure you ran `python manage.py seed_data`
- Check username/password: `ama` / `AsrAma12@#`

