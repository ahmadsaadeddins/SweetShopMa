#!/usr/bin/env python
"""
SweetShopMa Desktop Application Setup Script

This script initializes the project structure and prepares the environment.
"""

import os
import sys
import subprocess
from pathlib import Path

def create_directories():
    """Create all necessary directories"""
    print("[Setup] Creating directory structure...")
    
    directories = [
        'backend',
        'backend/sweetshop',
        'backend/api',
        'backend/api/migrations',
        'frontend',
        'frontend/templates',
        'frontend/static',
        'frontend/static/css',
        'frontend/static/js',
        'frontend/static/images',
        'security',
        'sync',
        'scripts',
        'logs',
        'backups',
    ]
    
    for directory in directories:
        os.makedirs(directory, exist_ok=True)
        print(f"  ✓ Created: {directory}/")
    
    print("[Setup] ✓ Directory structure created\n")

def create_init_files():
    """Create __init__.py files for Python packages"""
    print("[Setup] Creating Python package files...")
    
    init_files = [
        'backend/__init__.py',
        'backend/sweetshop/__init__.py',
        'backend/api/__init__.py',
        'frontend/__init__.py',
        'security/__init__.py',
        'sync/__init__.py',
        'scripts/__init__.py',
    ]
    
    for init_file in init_files:
        filepath = Path(init_file)
        if not filepath.exists():
            filepath.touch()
            print(f"  ✓ Created: {init_file}")
    
    print("[Setup] ✓ Python package files created\n")

def create_env_file():
    """Create .env file for environment variables"""
    print("[Setup] Creating .env file...")
    
    env_content = """# SweetShopMa Desktop Environment Variables

# Branch Configuration
BRANCH_ID=branch-1
BRANCH_NAME=Main Branch

# Server Configuration
CENTRAL_SERVER_URL=https://your-server.com/api
SYNC_INTERVAL=300

# Django Configuration
DJANGO_SECRET_KEY=django-insecure-change-this-in-production
DJANGO_DEBUG=True

# Database Configuration
DB_NAME=sweetshopma.db

# Security
LICENSE_FILE=license.key
"""
    
    with open('.env', 'w') as f:
        f.write(env_content)
    
    print("  ✓ Created: .env")
    print("[Setup] ✓ .env file created\n")
    print("  ⚠ IMPORTANT: Edit .env and update the configuration values\n")

def install_dependencies():
    """Install Python dependencies"""
    print("[Setup] Installing dependencies...")
    print("  This may take a few minutes...\n")
    
    try:
        subprocess.run(
            [sys.executable, '-m', 'pip', 'install', '-r', 'requirements.txt'],
            check=True,
            capture_output=False
        )
        print("\n[Setup] ✓ Dependencies installed\n")
    except subprocess.CalledProcessError as e:
        print(f"\n[Setup] ✗ Failed to install dependencies: {e}\n")
        sys.exit(1)

def initialize_django():
    """Initialize Django project"""
    print("[Setup] Initializing Django project...")
    
    try:
        # Change to backend directory
        os.chdir('backend')
        
        # Create Django project
        subprocess.run(
            [sys.executable, '-m', 'django', 'startproject', 'sweetshop', '.'],
            check=True,
            capture_output=False
        )
        
        # Create API app
        subprocess.run(
            [sys.executable, 'manage.py', 'startapp', 'api'],
            check=True,
            capture_output=False
        )
        
        # Run initial migrations
        subprocess.run(
            [sys.executable, 'manage.py', 'migrate'],
            check=True,
            capture_output=False
        )
        
        # Change back to root directory
        os.chdir('..')
        
        print("[Setup] ✓ Django project initialized\n")
    except subprocess.CalledProcessError as e:
        print(f"\n[Setup] ✗ Failed to initialize Django: {e}\n")
        sys.exit(1)

def create_readme_link():
    """Create a link to README for quick reference"""
    print("[Setup] Setup complete!")
    print("\n" + "="*60)
    print("NEXT STEPS")
    print("="*60)
    print("\n1. Edit .env file with your configuration:")
    print("   - Set BRANCH_ID and BRANCH_NAME")
    print("   - Set CENTRAL_SERVER_URL")
    print("   - Update DJANGO_SECRET_KEY\n")
    
    print("2. Review the project structure:")
    print("   - Read README.md for detailed documentation")
    print("   - Check plans/ directory for implementation guides\n")
    
    print("3. Start development:")
    print("   - Backend: cd backend && python manage.py runserver")
    print("   - Frontend: cd frontend && python main.py\n")
    
    print("4. When ready to build:")
    print("   - Run: python scripts/build.py")
    print("   - Find executable in dist/SweetShopMa.exe\n")
    
    print("="*60)

def main():
    """Main setup function"""
    print("="*60)
    print("SweetShopMa Desktop Application Setup")
    print("="*60)
    print()
    
    # Check Python version
    if sys.version_info < (3, 9):
        print("[Setup] ✗ Python 3.9 or higher is required")
        print(f"[Setup]   Current version: {sys.version}")
        sys.exit(1)
    
    print(f"[Setup] Python version: {sys.version}\n")
    
    # Create directories
    create_directories()
    
    # Create __init__.py files
    create_init_files()
    
    # Create .env file
    create_env_file()
    
    # Ask about installing dependencies
    print("[Setup] Install dependencies now?")
    response = input("  Install dependencies? (y/n): ").strip().lower()
    
    if response == 'y':
        install_dependencies()
        
        # Ask about Django initialization
        print("\n[Setup] Initialize Django project now?")
        response = input("  Initialize Django? (y/n): ").strip().lower()
        
        if response == 'y':
            initialize_django()
    else:
        print("\n[Setup] Skipping dependency installation")
        print("  Run 'pip install -r requirements.txt' manually later\n")
    
    # Show next steps
    create_readme_link()

if __name__ == '__main__':
    main()
