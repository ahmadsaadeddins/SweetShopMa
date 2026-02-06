#!/usr/bin/env python
"""
Script to populate UserProfile for existing users.

This script assigns roles based on existing user flags:
- Superusers → Developer role
- Staff users (non-superuser) → Admin role
- Regular users → User role
"""

import os
import sys

# Add the project root to the Python path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# Set up Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')

import django
django.setup()

from django.contrib.auth.models import User
from django.db import connection


def populate_user_profiles():
    """Create UserProfile for all existing users using raw SQL."""
    print("Starting UserProfile population...")
    
    users = User.objects.all()
    print(f"Found {users.count()} users")
    
    created_count = 0
    skipped_count = 0
    
    # Check which users already have profiles
    existing_profile_user_ids = set()
    cursor = connection.cursor()
    cursor.execute("SELECT user_id FROM api_userprofile")
    for row in cursor.fetchall():
        existing_profile_user_ids.add(row[0])
    
    for user in users:
        # Check if profile already exists
        if user.id in existing_profile_user_ids:
            print(f"  User '{user.username}' (id={user.id}) already has a profile")
            skipped_count += 1
            continue
        
        # Determine role based on user flags
        if user.is_superuser:
            role = 'Developer'
        elif user.is_staff:
            role = 'Admin'
        else:
            role = 'Seller'  # Non-staff users are Sellers
        
        # Insert using raw SQL with proper format strings
        cursor.execute(
            "INSERT INTO api_userprofile (user_id, role, monthly_salary, overtime_multiplier, created_at, updated_at) VALUES (%s, %s, %s, %s, datetime('now'), datetime('now'))",
            [int(user.id), str(role), float(0.00), float(1.50)]
        )
        
        print(f"  Created profile for '{user.username}' with role '{role}'")
        created_count += 1
    
    print(f"\nDone! Created {created_count} profiles, skipped {skipped_count} (already existed)")


if __name__ == '__main__':
    populate_user_profiles()
