"""
Create a superuser for the SweetShopMa desktop application.
Run this script to create the default admin user.
"""

import os
import sys
import django

# Setup Django
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')
django.setup()

from django.contrib.auth.models import User

def create_superuser():
    """Create superuser if it doesn't exist"""
    username = 'admin'
    email = 'admin@sweetshop.ma'
    password = 'admin123'
    
    if User.objects.filter(username=username).exists():
        print(f"[OK] User '{username}' already exists")
        user = User.objects.get(username=username)
        print(f"    ID: {user.id}")
        print(f"    Username: {user.username}")
        print(f"    Email: {user.email}")
        print(f"    Staff: {user.is_staff}")
        print(f"    Superuser: {user.is_superuser}")
        return user
    
    user = User.objects.create_superuser(
        username=username,
        email=email,
        password=password
    )
    
    print(f"[OK] Superuser created successfully!")
    print(f"    Username: {username}")
    print(f"    Password: {password}")
    print(f"    Email: {email}")
    print(f"\nYou can now login with these credentials.")
    
    return user

if __name__ == '__main__':
    create_superuser()
