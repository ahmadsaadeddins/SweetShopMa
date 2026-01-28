---
description: Setup the Django backend for the web application
---

# Setup Django Backend

Setup and run the Django backend server for the SweetShopMa web application.

## Initial Setup

1. Navigate to backend directory:
```bash
cd c:\Users\AMA\Documents\myhub\SweetShopMa\web-app\backend
```

2. Create virtual environment:
```bash
python -m venv venv
```

3. Activate virtual environment (Windows):
```bash
venv\Scripts\activate
```

// turbo
4. Install dependencies:
```bash
pip install -r requirements.txt
```

// turbo
5. Run database migrations:
```bash
python manage.py migrate
```

6. Seed initial data (creates default user and sample products):
```bash
python manage.py seed_data
```

## Run Development Server

// turbo
7. Start Django server:
```bash
python manage.py runserver
```

Backend will run on `http://localhost:8000`

## Default Login Credentials
- **Username:** `ama`
- **Password:** `AsrAma12@#`
- **Role:** Developer (full access)

## Database Management

8. Create new migrations after model changes:
```bash
python manage.py makemigrations
```

9. Reset database (delete db.sqlite3 and run migrations/seed again):
```bash
del db.sqlite3
python manage.py migrate
python manage.py seed_data
```

## Create Superuser

10. Create a new admin user:
```bash
python manage.py createsuperuser
```
