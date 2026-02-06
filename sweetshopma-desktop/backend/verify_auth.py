#!/usr/bin/env python
"""Verify mostafa authentication works"""

import os
import sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')

import django
django.setup()

from django.contrib.auth import authenticate
from django.contrib.auth.models import User

print("=== Authentication Verification ===")

# Check mostafa
user = User.objects.get(username='mostafa')
print(f'User: {user.username}')
print(f'Is active: {user.is_active}')
print(f'Password hash: {user.password[:30]}...')

# Test authentication
auth = authenticate(username='mostafa', password='mostafa123')
print(f'Auth with mostafa123: {auth}')

if auth:
    print("\n✓ Authentication works! Restart the Django server and frontend to login.")
else:
    print("\n✗ Authentication failed!")
    # Try to find correct password
    print("\nTrying common passwords...")
    for pwd in ['mostafa123', '123456', 'password', 'mostafa', user.username]:
        auth = authenticate(username='mostafa', password=pwd)
        print(f'  {pwd}: {auth}')
