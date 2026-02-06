#!/usr/bin/env python
"""Debug authentication for mostafa user."""

import os
import sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')

import django
django.setup()

from django.contrib.auth import authenticate
from django.contrib.auth.models import User
from api.models import UserProfile

print("=== Debug Authentication ===")

# Check all users
print("\nAll users in database:")
for user in User.objects.all():
    print(f'  - {user.username}: active={user.is_active}, is_staff={user.is_staff}')

# Check password for admin (to know a working password)
print("\n=== Admin Authentication Test ===")
admin = User.objects.get(username='admin')
print(f'Admin password hash starts with: {admin.password[:20]}...')

# Try to find a working password for admin
for pwd in ['admin', 'admin123', 'password', '123456']:
    auth = authenticate(username='admin', password=pwd)
    print(f'Admin auth with "{pwd}": {auth}')

print("\n=== Mostafa Status ===")
try:
    user = User.objects.get(username='mostafa')
    print(f'User exists: {user.username}')
    print(f'Password hash: {user.password[:20] if user.password else None}...')
    print(f'Is active: {user.is_active}')
    
except User.DoesNotExist:
    print('User "mostafa" does NOT exist in auth_user table!')
    print('This means mostafa exists in api_userprofile but not in auth_user!')
    
    print("\nChecking UserProfile for mostafa:")
    try:
        up = UserProfile.objects.get(user__username='mostafa')
        print(f'UserProfile found: {up.user.username}, role: {up.role}')
        print(f'UserProfile user_id: {up.user_id}')
    except UserProfile.DoesNotExist:
        print('UserProfile for mostafa not found!')
