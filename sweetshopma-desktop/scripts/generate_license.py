#!/usr/bin/env python
"""
License Generator for SweetShopMa Desktop Application

Use this script to generate license keys for customers.
Keep this script private - do not distribute with the application!
"""

import sys
import os
from datetime import datetime

# Add parent directory to path for imports
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from security.license_manager import LicenseManager
from security.hardware_id import get_hardware_id


def print_banner():
    """Print application banner"""
    print("\n" + "="*60)
    print("SweetShopMa License Generator")
    print("="*60)


def get_user_input():
    """Get license information from user"""
    print("\n" + "-"*60)
    print("Enter License Information")
    print("-"*60)
    
    # Get hardware ID
    hw_id = input("\n1. Enter customer's Hardware ID: ").strip()
    if not hw_id:
        print("✗ Hardware ID is required!")
        return None
    
    # Validate hardware ID format (should be 64 hex characters)
    if len(hw_id) != 64 or not all(c in '0123456789abcdef' for c in hw_id.lower()):
        print("⚠ Warning: Hardware ID format looks unusual (expected 64 hex characters)")
        confirm = input("Continue anyway? (y/n): ").strip().lower()
        if confirm != 'y':
            return None
    
    # Get licensee name
    licensee = input("2. Enter customer/company name: ").strip()
    if not licensee:
        print("✗ Licensee name is required!")
        return None
    
    # Get license duration
    days_input = input("3. Enter license duration (days, or 0 for perpetual): ").strip()
    try:
        expiry_days = int(days_input) if days_input else None
        if expiry_days is not None and expiry_days < 0:
            print("✗ License duration cannot be negative!")
            return None
    except ValueError:
        print("✗ Invalid license duration!")
        return None
    
    # Get license type
    print("\n4. Select license type:")
    print("   1. Standard")
    print("   2. Trial")
    print("   3. Enterprise")
    
    type_choice = input("   Enter choice (1-3, default=1): ").strip() or "1"
    license_types = {
        '1': 'standard',
        '2': 'trial',
        '3': 'enterprise'
    }
    license_type = license_types.get(type_choice, 'standard')
    
    return {
        'hardware_id': hw_id,
        'licensee': licensee,
        'expiry_days': expiry_days if expiry_days and expiry_days > 0 else None,
        'license_type': license_type
    }


def generate_license(license_info):
    """
    Generate license key.
    
    Args:
        license_info (dict): License information
        
    Returns:
        str: Generated license key
    """
    print("\n" + "-"*60)
    print("Generating License...")
    print("-"*60)
    
    # Initialize license manager
    # WARNING: Use a secure key in production!
    license_mgr = LicenseManager()
    
    # Generate license
    license_key = license_mgr.generate_license(
        hardware_id=license_info['hardware_id'],
        expiry_days=license_info['expiry_days'],
        licensee_name=license_info['licensee'],
        license_type=license_info['license_type']
    )
    
    return license_key


def save_license(license_key, licensee):
    """
    Save license key to file.
    
    Args:
        license_key (str): The license key
        licensee (str): Licensee name (for filename)
        
    Returns:
        str: Path to saved license file
    """
    # Create filename from licensee name
    safe_name = "".join(c for c in licensee if c.isalnum() or c in (' ', '-', '_')).strip()
    filename = f"{safe_name}_license.key"
    
    # Save license
    license_mgr = LicenseManager()
    license_mgr.save_license(license_key, filename)
    
    return filename


def display_summary(license_info, license_key, filename):
    """Display license generation summary"""
    print("\n" + "="*60)
    print("LICENSE GENERATED SUCCESSFULLY")
    print("="*60)
    
    print(f"\nLicensee: {license_info['licensee']}")
    print(f"Hardware ID: {license_info['hardware_id']}")
    print(f"License Type: {license_info['license_type'].title()}")
    
    if license_info['expiry_days']:
        print(f"Duration: {license_info['expiry_days']} days")
    else:
        print(f"Duration: Perpetual (never expires)")
    
    print(f"\nLicense File: {filename}")
    print(f"License Key (first 100 chars): {license_key[:100]}...")
    
    print("\n" + "-"*60)
    print("NEXT STEPS")
    print("-"*60)
    print(f"1. Send '{filename}' to the customer")
    print("2. Instruct the customer to:")
    print("   - Place the file in the application directory")
    print("   - Ensure it's named 'license.key'")
    print("   - Restart the application")
    print("3. Keep a backup of the license file")
    
    print("\n" + "="*60)


def main():
    """Main license generation function"""
    print_banner()
    
    # Get user input
    license_info = get_user_input()
    if not license_info:
        print("\n✗ License generation cancelled.")
        return
    
    # Confirm
    print("\n" + "-"*60)
    print("Review License Information")
    print("-"*60)
    print(f"Hardware ID: {license_info['hardware_id']}")
    print(f"Licensee: {license_info['licensee']}")
    print(f"Duration: {'Perpetual' if not license_info['expiry_days'] else f'{license_info['expiry_days']} days'}")
    print(f"Type: {license_info['license_type'].title()}")
    
    confirm = input("\nGenerate license with these details? (y/n): ").strip().lower()
    if confirm != 'y':
        print("\n✗ License generation cancelled.")
        return
    
    # Generate license
    try:
        license_key = generate_license(license_info)
        filename = save_license(license_key, license_info['licensee'])
        display_summary(license_info, license_key, filename)
        
    except Exception as e:
        print(f"\n✗ Error generating license: {e}")
        import traceback
        traceback.print_exc()


if __name__ == '__main__':
    main()
