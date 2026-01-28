---
description: Run the full web stack (backend + frontend) together
---

# Run Full Stack Web Application

Run both Django backend and Next.js frontend for complete web application development.

// turbo-all

## Step 1: Start Backend Server

1. Open a new terminal and navigate to backend:
```bash
cd c:\Users\AMA\Documents\myhub\SweetShopMa\web-app\backend
```

2. Activate virtual environment (if not already):
```bash
venv\Scripts\activate
```

3. Start Django server:
```bash
python manage.py runserver
```

Backend runs on: `http://localhost:8000`

## Step 2: Start Frontend Server

4. Open another terminal and navigate to frontend:
```bash
cd c:\Users\AMA\Documents\myhub\SweetShopMa\web-app\frontend
```

5. Start Next.js development server:
```bash
npm run dev
```

Frontend runs on: `http://localhost:3000`

## Accessing the Application

6. Open browser and go to: `http://localhost:3000`

7. Login with default credentials:
   - **Username:** `ama`
   - **Password:** `AsrAma12@#`

## API Endpoints

- `POST /api/auth/login/` - Login
- `GET /api/auth/login/` - Get current user
- `GET /api/products/` - List products
- `POST /api/products/{id}/restock/` - Restock product
- `GET /api/cart/` - Get cart items
- `POST /api/cart/` - Add to cart
- `POST /api/orders/checkout/` - Checkout
- `GET /api/reports/` - Get sales reports

## Troubleshooting

**Frontend can't connect to backend:**
- Check that Django server is running on port 8000
- Verify `NEXT_PUBLIC_API_URL=http://localhost:8000/api` in `.env.local`
- Check CORS settings in `backend/sweetshop/settings.py`

**Login fails:**
- Make sure you ran `python manage.py seed_data`
- Check username/password: `ama` / `AsrAma12@#`
