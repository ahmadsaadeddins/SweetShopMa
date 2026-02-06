#!/usr/bin/env python
"""Reset mostafa password to mostafa123"""

import os
import sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')

import django
django.setup()

from django.contrib.auth.models import User

print("=== Reset Password for mostafa ===")

try:
    user = User.objects.get(username='mostafa')
    user.set_password('mostafa123')
    user.save()
    print(f"Password reset successfully for user: {user.username}")
    
    # Verify
    from django.contrib.auth import authenticate
    auth = authenticate(username='mostafa', password='mostafa123')
    print(f"Authentication test with 'mostafa123': {auth}")
    
except User.DoesNotExist:
    print('User "mostafa" does not exist!')
