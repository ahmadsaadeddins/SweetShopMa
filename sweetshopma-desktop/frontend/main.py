"""
SweetShopMa Desktop Application - Main Entry Point

This is the main entry point for the desktop application.
It handles:
- Security checks (anti-debugging, license validation)
- Django backend startup
- PyWebView window creation
- Application lifecycle management
- Sync service management

Usage:
    Production: py main.py
    Development: py main.py --dev (requires npm run dev running separately)
"""

import sys
import os
import webview
import subprocess
import threading
import time
import argparse
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent))

from security.anti_debug import detect_debugger
from security.hardware_id import get_hardware_id, display_hardware_id
from security.license_manager import LicenseManager
from sync.sync_service import SyncService
from config.settings import (
    BRANCH_ID,
    BRANCH_NAME,
    WINDOW_WIDTH,
    WINDOW_HEIGHT,
    WINDOW_MIN_WIDTH,
    WINDOW_MIN_HEIGHT,
    WINDOW_BACKGROUND_COLOR,
    ENABLE_AUTO_SYNC,
)

# Global variables
django_process = None
api_bridge = None
window = None
sync_service = None


class Application:
    """Main application class"""
    
    def __init__(self, dev_mode=False):
        """Initialize application"""
        self.api_bridge = None
        self.window = None
        self.django_thread = None
        self.django_ready = threading.Event()
        self.sync_service = None
        self.dev_mode = dev_mode
    
    def check_security(self):
        """
        Perform all security checks before launching.
        
        Returns:
            bool: True if all checks pass
        """
        print("\n" + "="*60)
        print("SECURITY CHECKS")
        print("="*60)
        
        # 1. Anti-debugging check
        print("\n1. Checking for debugger...")
        try:
            detect_debugger()
            print("   ✓ No debugger detected")
        except SystemExit:
            print("   ✗ Debugger detected - exiting")
            return False
        
        # 2. Get hardware ID
        print("\n2. Getting hardware ID...")
        try:
            hw_id = get_hardware_id()
            print(f"   ✓ Hardware ID: {hw_id[:16]}...")
        except Exception as e:
            print(f"   ✗ Error getting hardware ID: {e}")
            return False
        
        # 3. Check license
        print("\n3. Validating license...")
        license_mgr = LicenseManager()
        license_key = license_mgr.load_license()
        
        if not license_key:
            print("   ✗ No license file found")
            display_hardware_id()
            return False
        
        is_valid, message, license_info = license_mgr.validate_license(
            license_key,
            hw_id
        )
        
        if not is_valid:
            print(f"   ✗ License validation failed: {message}")
            display_hardware_id()
            return False
        
        print(f"   ✓ License valid: {license_info.get('licensee', 'Unknown')}")
        
        if license_info.get('expiry_date'):
            print(f"   Expires: {license_info['expiry_date']}")
        else:
            print("   Expires: Never (perpetual)")
        
        print("\n" + "="*60)
        print("All security checks passed!")
        print("="*60 + "\n")
        
        return True
    
    def start_django(self):
        """Start Django backend in a separate thread"""
        print("[App] Starting Django backend...")
        
        def run_django():
            """Run Django server"""
            backend_dir = Path(__file__).parent.parent / 'backend'
            manage_py = backend_dir / 'manage.py'
            
            try:
                # Change to backend directory
                os.chdir(backend_dir)
                
                # Set DEBUG environment variable for Django
                env = os.environ.copy()
                env['DJANGO_DEBUG'] = 'True'
                
                # Run Django server with DEBUG enabled
                subprocess.run(
                    [sys.executable, str(manage_py), 'runserver', '127.0.0.1:8000'],
                    env=env,
                    check=True
                )
            except subprocess.CalledProcessError as e:
                print(f"[App] Django error: {e}")
            except Exception as e:
                print(f"[App] Django error: {e}")
        
        # Start Django in background thread
        self.django_thread = threading.Thread(target=run_django, daemon=True)
        self.django_thread.start()
        
        # Wait for Django to be ready
        print("[App] Waiting for Django to start...")
        for i in range(30):  # Wait up to 30 seconds
            try:
                import requests
                # Try root URL first (simpler check)
                response = requests.get('http://127.0.0.1:8000/', timeout=1)
                print(f"[App] Health check attempt {i+1}: Status {response.status_code}")
                if response.status_code in [200, 404]:  # 404 is ok - means server is running
                    print("[App] ✓ Django is ready")
                    self.django_ready.set()
                    return True
            except requests.exceptions.ConnectionError as e:
                print(f"[App] Attempt {i+1}: Connection error - {e}")
                time.sleep(1)
            except requests.exceptions.Timeout as e:
                print(f"[App] Attempt {i+1}: Timeout - {e}")
                time.sleep(1)
            except Exception as e:
                print(f"[App] Attempt {i+1}: Unexpected error - {type(e).__name__}: {e}")
                time.sleep(1)
        
        print("[App] ✗ Django failed to start after 30 attempts")
        return False
    
    def create_window(self):
        """Create PyWebView window with React build or dev server"""
        print("[App] Creating application window...")
        
        # Import API bridge
        from api import ApiBridge
        self.api_bridge = ApiBridge()
        
        # Determine URL based on mode
        if self.dev_mode:
            # Development mode - use Vite dev server
            app_url = 'http://localhost:3000'
            print(f"[App] Development mode - using Vite dev server: {app_url}")
            print("[App] Make sure 'npm run dev' is running in another terminal!")
        else:
            # Production mode - use built files
            build_dir = Path(__file__).parent / 'build'
            index_html = build_dir / 'index.html'
            
            if not index_html.exists():
                raise FileNotFoundError(
                    f"React build not found at {index_html}. "
                    "Run 'npm run build' in the frontend directory first."
                )
            
            print(f"[App] Production mode - loading React build from: {build_dir}")
            
            # Convert to file:// URL for proper relative path resolution
            import pathlib
            app_url = index_html.resolve().as_uri()
            print(f"[App] Using file URL: {app_url}")
        
        # Create window with React app
        self.window = webview.create_window(
            'SweetShopMa Desktop',
            url=app_url,
            js_api=self.api_bridge,
            width=WINDOW_WIDTH,
            height=WINDOW_HEIGHT,
            min_size=(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT),
            background_color=WINDOW_BACKGROUND_COLOR,
        )
        
        print("[App] ✓ Window created with React app")
    
    def start_sync_service(self):
        """Start the sync service if enabled"""
        if not ENABLE_AUTO_SYNC:
            print("[App] Auto-sync is disabled")
            return
        
        try:
            print("[App] Starting sync service...")
            self.sync_service = SyncService()
            self.sync_service.start()
            print("[App] ✓ Sync service started")
        except Exception as e:
            print(f"[App] Warning: Could not start sync service: {e}")
    
    def stop_sync_service(self):
        """Stop the sync service"""
        if self.sync_service:
            print("[App] Stopping sync service...")
            self.sync_service.stop()
            print("[App] ✓ Sync service stopped")
    
    def start(self):
        """Start the application"""
        print("\n" + "="*60)
        print("SweetShopMa Desktop Application")
        print("="*60)
        print(f"Branch ID: {BRANCH_ID or 'Not configured'}")
        print(f"Branch Name: {BRANCH_NAME or 'Not configured'}")
        print("="*60 + "\n")
        
        # 1. Security checks
        if not self.check_security():
            print("\n" + "="*60)
            print("APPLICATION CANNOT START")
            print("="*60)
            print("Security checks failed. Please resolve the issues above.")
            print("="*60)
            input("\nPress Enter to exit...")
            sys.exit(1)
        
        # 2. Start Django backend
        if not self.start_django():
            print("\n" + "="*60)
            print("APPLICATION CANNOT START")
            print("="*60)
            print("Django backend failed to start.")
            print("Please check the error messages above.")
            print("="*60)
            input("\nPress Enter to exit...")
            sys.exit(1)
        
        # 3. Start sync service
        self.start_sync_service()
        
        # 4. Create window
        self.create_window()
        
        # 5. Start PyWebView
        print("[App] Starting PyWebView...")
        print("="*60 + "\n")
        
        try:
            webview.start(debug=False)
        except Exception as e:
            print(f"[App] Error: {e}")
            sys.exit(1)
        finally:
            # Stop sync service on exit
            self.stop_sync_service()
        
        print("\n[App] Application closed")


def main():
    """Main entry point"""
    # Parse command line arguments
    parser = argparse.ArgumentParser(
        description='SweetShopMa Desktop Application',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  Production mode (uses built files):
    py main.py
    
  Development mode (uses Vite dev server):
    py main.py --dev
    (Make sure to run 'npm run dev' in another terminal first)
        """
    )
    parser.add_argument(
        '--dev',
        action='store_true',
        help='Run in development mode (uses Vite dev server at http://localhost:3000)'
    )
    
    args = parser.parse_args()
    
    # Create and start application
    app = Application(dev_mode=args.dev)
    app.start()


if __name__ == '__main__':
    main()
