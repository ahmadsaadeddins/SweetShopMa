#!/usr/bin/env python
"""
SweetShopMa Backend Server
Entry point for the Django backend server executable.
"""

import os
import sys
from pathlib import Path

# Determine if running as frozen executable
if getattr(sys, 'frozen', False):
    FROZEN = True
    BUNDLE_DIR = Path(sys._MEIPASS)
    EXE_DIR = Path(sys.executable).parent
else:
    FROZEN = False
    BUNDLE_DIR = Path(__file__).parent
    EXE_DIR = Path(__file__).parent

# Add bundle directory to path
sys.path.insert(0, str(BUNDLE_DIR))

from security.anti_debug import detect_debugger
from security.hardware_id import get_hardware_id, display_hardware_id
from security.license_manager import LicenseManager


def check_license():
    """Validate license before starting server"""
    print("\n" + "="*60)
    print("SWEETSHOPMA BACKEND SERVER")
    print("="*60)
    
    # 1. Anti-debugging check
    print("\n1. Checking for debugger...")
    try:
        detect_debugger()
        print("   [OK] No debugger detected")
    except SystemExit:
        print("   [X] Debugger detected - exiting")
        return False
    
    # 2. Get hardware ID
    print("\n2. Getting hardware ID...")
    try:
        hw_id = get_hardware_id()
        print(f"   [OK] Hardware ID: {hw_id[:16]}...")
    except Exception as e:
        print(f"   [X] Error getting hardware ID: {e}")
        return False
    
    # 3. Check license
    print("\n3. Validating license...")
    license_mgr = LicenseManager()
    license_key = license_mgr.load_license()
    
    if not license_key:
        print("   [X] No license file found")
        display_hardware_id()
        return False
    
    is_valid, message, license_info = license_mgr.validate_license(
        license_key,
        hw_id
    )
    
    if not is_valid:
        print(f"   [X] License validation failed: {message}")
        display_hardware_id()
        return False
    
    print(f"   [OK] License valid: {license_info.get('licensee', 'Unknown')}")
    
    if license_info.get('expiry_date'):
        print(f"   Expires: {license_info['expiry_date']}")
    else:
        print("   Expires: Never (perpetual)")
    
    print("\n" + "="*60)
    print("License check passed - starting Django server")
    print("="*60 + "\n")
    
    return True


def start_django_server():
    """Start Django development server"""
    # Set up paths
    backend_dir = BUNDLE_DIR / 'backend'
    db_path = EXE_DIR / 'sweetshopma.db'
    
    # Change to backend directory
    os.chdir(backend_dir)
    
    # Set environment variables
    os.environ['DJANGO_DEBUG'] = 'True'
    os.environ['SWEETSHOP_DB_PATH'] = str(db_path)
    os.environ['SWEETSHOP_EXE_DIR'] = str(EXE_DIR)
    os.environ['DJANGO_SETTINGS_MODULE'] = 'sweetshop.settings'
    
    # Load .env file from EXE directory
    env_file = EXE_DIR / '.env'
    print(f"[Backend] Looking for .env at: {env_file}")
    
    if env_file.exists():
        print(f"[Backend] Loading environment variables from .env")
        try:
            with open(env_file, 'r', encoding='utf-8') as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        key, value = line.split('=', 1)
                        os.environ[key.strip()] = value.strip()
                        if key.strip() == 'DJANGO_SECRET_KEY':
                            print("[Backend] Loaded DJANGO_SECRET_KEY")
        except Exception as e:
            print(f"[Backend] Error loading .env: {e}")
    else:
        print("[Backend] Warning: .env file not found!")

    print(f"[Backend] Backend dir: {backend_dir}")
    print(f"[Backend] Database path: {db_path}")
    print(f"[Backend] Starting Django on http://127.0.0.1:8000")
    
    # Add backend to path
    if str(backend_dir) not in sys.path:
        sys.path.insert(0, str(backend_dir))
    
    # Import and run Django
    import django
    django.setup()
    
    from django.core.management import execute_from_command_line
    
    # Run migrations
    print("[Backend] Running database migrations...")
    try:
        # Debug: Show migrations status
        print("[Backend] Migration status before migrate:")
        import io
        from django.core.management import call_command
        
        # Capture showmigrations output
        out = io.StringIO()
        call_command('showmigrations', stdout=out)
        print(out.getvalue())
        
        execute_from_command_line(['manage.py', 'migrate', '--noinput'])
        print("[Backend] Migrations completed")
    except Exception as e:
        print(f"[Backend] Error running migrations: {e}")
    
    # Create default superuser and initial data
    try:
        from django.contrib.auth import get_user_model
        User = get_user_model()
        
        if not User.objects.filter(is_superuser=True).exists():
            print("[Backend] Creating default superuser (admin/admin)...")
            User.objects.create_superuser('admin', 'admin@example.com', 'admin')
            print("[Backend] Default superuser created")
            
        # You can add more initial data population here if needed
        
    except Exception as e:
        print(f"[Backend] Error creating default data: {e}")
    
    print("[Backend] Starting server...")
    execute_from_command_line(['manage.py', 'runserver', '127.0.0.1:8000', '--noreload'])


def main():
    """Main entry point"""
    try:
        # Check license first
        if not check_license():
            if FROZEN:
                import ctypes
                ctypes.windll.user32.MessageBoxW(
                    0,
                    "Backend license validation failed.\n\nPlease ensure license.key is valid.",
                    "SweetShopMa Backend - License Required",
                    0x10
                )
            else:
                input("\nPress Enter to exit...")
            sys.exit(1)
        
        # Start Django server
        start_django_server()
        
    except KeyboardInterrupt:
        print("\n[Backend] Server stopped by user")
        sys.exit(0)
    except Exception as e:
        import traceback
        error_msg = f"[Backend] Error: {e}\n{traceback.format_exc()}"
        print(error_msg)
        
        # Write error to log
        try:
            error_log = EXE_DIR / 'backend_error.log'
            with open(error_log, 'w', encoding='utf-8') as f:
                f.write(error_msg)
            print(f"[Backend] Error log written to: {error_log}")
        except:
            pass
        
        if FROZEN:
            import ctypes
            ctypes.windll.user32.MessageBoxW(
                0,
                f"Backend server error:\n\n{str(e)}\n\nCheck backend_error.log for details.",
                "SweetShopMa Backend - Error",
                0x10
            )
        else:
            input("\nPress Enter to exit...")
        sys.exit(1)


if __name__ == '__main__':
    main()
