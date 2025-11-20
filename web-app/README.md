# SweetShopMa Web Application

This is a web version of the SweetShopMa Point of Sale system, built with:
- **Backend**: Django 5.0+ with Django REST Framework
- **Frontend**: Next.js 16 with TypeScript and Tailwind CSS

## Project Structure

```
web-app/
├── backend/          # Django backend API
│   ├── sweetshop/    # Django project
│   ├── api/          # REST API app
│   └── requirements.txt
├── frontend/         # Next.js frontend
│   ├── app/          # Next.js app directory
│   ├── components/   # React components
│   ├── lib/          # Utilities and API client
│   └── package.json
└── README.md         # This file
```

## Features

- ✅ User authentication with role-based access control
- ✅ Product management (unit-based and weight-based)
- ✅ Shopping cart and checkout
- ✅ Order management and history
- ✅ Admin panel (user management, reports)
- ✅ Attendance tracking
- ✅ Restock tracking
- ✅ Multi-language support (English/Arabic with RTL)
- ✅ Receipt printing (web-based)
- ✅ Sales reports and analytics

## Quick Start

### Backend Setup

```bash
cd backend
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
python manage.py migrate
python manage.py createsuperuser
python manage.py runserver
```

### Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

The app will be available at:
- Frontend: http://localhost:3000
- Backend API: http://localhost:8000
- Django Admin: http://localhost:8000/admin

## Default Credentials

After running migrations, a default Developer user is created:
- Username: `ama`
- Password: `AsrAma12@#`

## Documentation

See individual README files in `backend/` and `frontend/` directories for detailed setup instructions.

