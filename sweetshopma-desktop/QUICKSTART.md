# SweetShopMa Desktop - Quick Start Guide

## Phase 1: Initial Setup (5 minutes)

### Step 1: Run Setup Script

```bash
# Navigate to project directory
cd sweetshopma-desktop

# Run setup script
python scripts/setup.py
```

This will:
- ✅ Create all necessary directories
- ✅ Create Python package files
- ✅ Create .env configuration file
- ✅ Install dependencies (optional)
- ✅ Initialize Django project (optional)

### Step 2: Configure Environment

Edit the `.env` file:

```bash
# Branch Configuration
BRANCH_ID=branch-1
BRANCH_NAME=Main Branch

# Server Configuration
CENTRAL_SERVER_URL=https://your-server.com/api

# Security
DJANGO_SECRET_KEY=generate-secure-random-key-here
```

---

## Phase 2: Development Setup (10 minutes)

### Option A: Quick Start (Recommended)

```bash
# Install dependencies
pip install -r requirements.txt

# Run setup script with all options
python scripts/setup.py
# Choose 'y' for both questions
```

### Option B: Manual Setup

```bash
# 1. Create virtual environment
python -m venv venv

# 2. Activate virtual environment
# Windows:
venv\Scripts\activate
# Mac/Linux:
source venv/bin/activate

# 3. Install dependencies
pip install -r requirements.txt

# 4. Initialize Django
cd backend
python manage.py startproject sweetshop .
python manage.py startapp api
python manage.py migrate
cd ..
```

---

## Phase 3: Run in Development Mode

### Terminal 1: Start Django Backend

```bash
cd backend
python manage.py runserver
```

You should see:
```
Django version 4.2.x, using settings 'sweetshop.settings'
Starting development server at http://127.0.0.1:8000/
```

### Terminal 2: Start PyWebView Frontend

```bash
# In a new terminal
cd frontend
python main.py
```

The desktop application window should open.

---

## Phase 4: Build Executable (When Ready)

### Step 1: Obfuscate Code (Optional but Recommended)

```bash
python scripts/obfuscate.py
```

### Step 2: Build with PyInstaller

```bash
python scripts/build.py
```

### Step 3: Find Executable

```
dist/SweetShopMa.exe
```

---

## Phase 5: Deploy to Branch

### For Each Branch:

1. **Copy executable to branch PC**
   ```
   Copy dist/SweetShopMa.exe to target computer
   ```

2. **Run to get Hardware ID**
   ```
   Double-click SweetShopMa.exe
   Note the Hardware ID displayed
   ```

3. **Generate license**
   ```bash
   python scripts/generate_license.py
   Enter Hardware ID: [paste from step 2]
   Enter branch name: Branch 1
   Enter duration: 365
   ```

4. **Distribute license**
   ```
   Copy the generated .key file to branch PC
   Place next to SweetShopMa.exe
   ```

5. **Run application**
   ```
   Double-click SweetShopMa.exe
   Should launch successfully
   ```

---

## Common Commands

### Development

```bash
# Start Django backend
cd backend && python manage.py runserver

# Start PyWebView frontend
cd frontend && python main.py

# Run Django tests
cd backend && python manage.py test

# Create Django migrations
cd backend && python manage.py makemigrations

# Apply migrations
cd backend && python manage.py migrate
```

### Build

```bash
# Obfuscate code
python scripts/obfuscate.py

# Build executable
python scripts/build.py

# Generate license
python scripts/generate_license.py
```

### Database

```bash
# Open Django shell
cd backend && python manage.py shell

# Reset database (WARNING: Deletes all data)
cd backend && rm ../sweetshopma.db
python manage.py migrate

# Create superuser
cd backend && python manage.py createsuperuser
```

---

## Troubleshooting

### "Module not found" error

```bash
# Make sure virtual environment is activated
# Windows:
venv\Scripts\activate
# Mac/Linux:
source venv/bin/activate

# Reinstall dependencies
pip install -r requirements.txt
```

### "License not valid" error

- Verify Hardware ID matches exactly
- Regenerate license with correct Hardware ID
- Ensure license.key is in same directory as .exe

### Django won't start

```bash
# Check if port 8000 is already in use
# Windows:
netstat -ano | findstr :8000
# Mac/Linux:
lsof -i :8000

# Use different port
cd backend && python manage.py runserver 8001
```

### Database errors

```bash
# Delete database and start fresh
rm sweetshopma.db
cd backend && python manage.py migrate
```

---

## Next Steps

1. **Read the full documentation**: Check `README.md` for detailed information
2. **Review implementation plans**: Check `plans/` directory for architecture details
3. **Customize for your needs**: Modify models, views, and templates
4. **Set up central server**: Deploy sync server and owner dashboard
5. **Test thoroughly**: Test offline and online functionality

---

## File Structure Reference

```
sweetshopma-desktop/
├── backend/              # Django backend
│   ├── sweetshop/       # Django project
│   └── api/             # API app
├── frontend/            # PyWebView frontend
│   ├── main.py         # Entry point
│   ├── api.py          # API bridge
│   └── templates/      # HTML templates
├── security/           # Security modules
├── sync/              # Sync service
├── config/            # Configuration
├── scripts/           # Build scripts
└── dist/              # Built executable (after build)
```

---

## Support

For detailed information:
- `README.md` - Full documentation
- `plans/complete-implementation-plan.md` - Step-by-step implementation
- `plans/multi-branch-architecture.md` - Architecture overview

For issues:
- Check `logs/` directory for error logs
- Verify all dependencies are installed
- Ensure Python 3.9+ is being used
