#!/usr/bin/env python
"""
Generate a secure Django secret key for SweetShopMa Desktop Application.

This script generates a cryptographically secure random secret key
that should be set as the DJANGO_SECRET_KEY environment variable.

Usage:
    python generate_secret_key.py

Then set the environment variable:
    Windows: set DJANGO_SECRET_KEY=<generated_key>
    Linux/Mac: export DJANGO_SECRET_KEY=<generated_key>
"""

import secrets
import sys
import os


def generate_secret_key():
    """Generate a secure random secret key for Django."""
    # Generate 50 bytes of random data and encode as hex
    # This is similar to Django's get_random_secret_key()
    return secrets.token_hex(50)


def main():
    """Main function to generate and display the secret key."""
    print("=" * 70)
    print("SweetShopMa Desktop - Secret Key Generator")
    print("=" * 70)
    print()

    key = generate_secret_key()

    print("Generated Secret Key:")
    print("-" * 70)
    print(key)
    print("-" * 70)
    print()

    print("IMPORTANT: Set this as an environment variable before running the app!")
    print()
    print("Windows Command Prompt:")
    print(f"    set DJANGO_SECRET_KEY={key}")
    print()
    print("Windows PowerShell:")
    print(f"    $env:DJANGO_SECRET_KEY='{key}'")
    print()
    print("Linux/Mac:")
    print(f"    export DJANGO_SECRET_KEY='{key}'")
    print()
    print("For permanent setup, add to your environment variables or .env file:")
    print(f"    DJANGO_SECRET_KEY={key}")
    print()

    # Ask if user wants to save to .env file
    response = input("Would you like to save this to a .env file? (y/n): ").strip().lower()
    if response == 'y':
        env_path = Path(__file__).parent / '.env'
        with open(env_path, 'w') as f:
            f.write(f"# SweetShopMa Desktop Environment Variables\n")
            f.write(f"# Generated: {__import__('datetime').datetime.now()}\n")
            f.write(f"DJANGO_SECRET_KEY={key}\n")
            f.write(f"DJANGO_DEBUG=True\n")
        print(f"✓ Saved to: {env_path}")
        print()
        print("Note: When using .env file, make sure to load it in your application.")
        print("You may need to install 'python-dotenv' package.")


if __name__ == '__main__':
    from pathlib import Path
    main()
