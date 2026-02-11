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
import threading
import time
import argparse
import socket
from pathlib import Path
from http.server import HTTPServer, SimpleHTTPRequestHandler

# Determine if running as frozen executable (PyInstaller)
if getattr(sys, 'frozen', False):
    # Running as compiled EXE
    FROZEN = True
    # PyInstaller extracts to a temp directory
    BUNDLE_DIR = Path(sys._MEIPASS)
    # The actual EXE location (for license.key, database, etc.)
    EXE_DIR = Path(sys.executable).parent
else:
    # Running as script
    FROZEN = False
    BUNDLE_DIR = Path(__file__).parent.parent
    EXE_DIR = Path(__file__).parent.parent

# Add bundle directory to path for imports
sys.path.insert(0, str(BUNDLE_DIR))

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


def find_free_port():
    """Find a free port on localhost"""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('', 0))
        s.listen(1)
        port = s.getsockname()[1]
    return port


class CORSRequestHandler(SimpleHTTPRequestHandler):
    """Custom handler to enable CORS for local file serving"""
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET')
        self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate')
        return super().end_headers()

    def log_message(self, format, *args):
        # Suppress logging to keep console clean
        pass


def start_frontend_server(build_dir, port):
    """Start a simple HTTP server to serve the frontend build"""
    # Change to build directory so root path / serves index.html
    os.chdir(build_dir)
    
    server_address = ('127.0.0.1', port)
    httpd = HTTPServer(server_address, CORSRequestHandler)
    print(f"[App] Frontend server running at http://127.0.0.1:{port}")
    httpd.serve_forever()


class Application:
    """Main application class"""
    
    def __init__(self, dev_mode=False):
        """Initialize application"""
        self.api_bridge = None
        self.window = None
        self.django_thread = None
        self.django_ready = threading.Event()
        self.django_error = threading.Event()
        self.django_error_msg = ""
        self.sync_service = None
        self.dev_mode = dev_mode
        self.hardware_id = None  # Cache hardware ID
        self.frontend_server = None
        self.frontend_thread = None
    
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
            print("   [OK] No debugger detected")
        except SystemExit:
            print("   [X] Debugger detected - exiting")
            return False
        
        # 2. Get hardware ID
        print("\n2. Getting hardware ID...")
        try:
            self.hardware_id = get_hardware_id()
            print(f"   [OK] Hardware ID: {self.hardware_id[:16]}...")
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
            self.hardware_id
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
        print("All security checks passed!")
        print("="*60 + "\n")
        
        return True
    
    def _run_django_server(self):
        """Run Django server in background thread (called by start_django)"""
        try:
            # Set up paths
            backend_dir = BUNDLE_DIR / 'backend'
            db_path = EXE_DIR / 'sweetshopma.db'

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

            # Change to backend directory BEFORE django.setup()
            os.chdir(backend_dir)

            # Add backend to path
            if str(backend_dir) not in sys.path:
                sys.path.insert(0, str(backend_dir))

            # Import and setup Django
            import django
            django.setup()

            from django.core.management import execute_from_command_line

            # Run migrations
            print("[Backend] Running database migrations...")
            try:
                execute_from_command_line(['manage.py', 'migrate', '--noinput'])
                print("[Backend] Migrations completed")
            except Exception as e:
                print(f"[Backend] Error running migrations: {e}")

            # Create default superuser
            try:
                from django.contrib.auth import get_user_model
                User = get_user_model()
                if not User.objects.filter(is_superuser=True).exists():
                    print("[Backend] Creating default superuser (admin/admin)...")
                    User.objects.create_superuser('admin', 'admin@example.com', 'admin')
                    print("[Backend] Default superuser created")
            except Exception as e:
                print(f"[Backend] Error creating default data: {e}")

            print("[Backend] Starting server on http://127.0.0.1:8000")
            execute_from_command_line(['manage.py', 'runserver', '127.0.0.1:8000', '--noreload'])

        except Exception as e:
            import traceback
            error_msg = f"{e}\n{traceback.format_exc()}"
            print(f"[Backend] Error in Django thread: {error_msg}")
            # Write error log next to EXE
            try:
                error_log = EXE_DIR / 'backend_error.log'
                with open(error_log, 'w', encoding='utf-8') as f:
                    f.write(error_msg)
                print(f"[Backend] Error log written to: {error_log}")
            except:
                pass
            # Signal the error to the main thread
            self.django_error_msg = str(e)
            self.django_error.set()

    def start_django(self):
        """Start Django backend in a background thread (in-process)"""
        print("[App] Starting Django backend (in-process)...")

        # Check if backend is already running
        try:
            import requests
            response = requests.get('http://127.0.0.1:8000/', timeout=1)
            if response.status_code in [200, 404]:
                print("[App] [OK] Backend already running")
                self.django_ready.set()
                return True
        except:
            pass  # Backend not running yet

        # Start Django in a daemon thread
        self.django_thread = threading.Thread(
            target=self._run_django_server,
            daemon=True,
            name='DjangoServer'
        )
        self.django_thread.start()
        print("[App] Django thread started")

        # Wait for Django to be ready
        print("[App] Waiting for Django to start...")
        for i in range(30):  # Wait up to 30 seconds
            # Check if the thread crashed
            if self.django_error.is_set():
                print(f"[App] [X] Django thread crashed: {self.django_error_msg}")
                return False
            try:
                import requests
                response = requests.get('http://127.0.0.1:8000/', timeout=1)
                print(f"[App] Health check attempt {i+1}: Status {response.status_code}")
                if response.status_code in [200, 404]:
                    print("[App] [OK] Django is ready")
                    self.django_ready.set()
                    return True
            except requests.exceptions.ConnectionError:
                print(f"[App] Attempt {i+1}: Connection error - waiting...")
                time.sleep(1)
            except requests.exceptions.Timeout:
                print(f"[App] Attempt {i+1}: Timeout - waiting...")
                time.sleep(1)
            except Exception as e:
                print(f"[App] Attempt {i+1}: {type(e).__name__}: {e}")
                time.sleep(1)

        print("[App] [X] Django failed to start after 30 attempts")
        return False
    
    def create_window(self):
        """Create PyWebView window with React build or dev server"""
        print("[App] Creating application window...")
        
        # Import API bridge (renamed to js_api to avoid collision with backend.api)
        from js_api import ApiBridge
        self.api_bridge = ApiBridge()
        
        # Determine URL based on mode
        if self.dev_mode:
            # Development mode - use Vite dev server
            app_url = 'http://localhost:3000'
            print(f"[App] Development mode - using Vite dev server: {app_url}")
            print("[App] Make sure 'npm run dev' is running in another terminal!")
        else:
            # Production mode - use built files served via HTTP server
            # Use BUNDLE_DIR to ensure it works in both frozen and dev modes
            build_dir = BUNDLE_DIR / 'frontend' / 'build'
            
            if not (build_dir / 'index.html').exists():
                raise FileNotFoundError(
                    f"React build not found at {build_dir / 'index.html'}. "
                    "Run 'npm run build' in the frontend directory first."
                )
            
            print(f"[App] Production mode - serving React build from: {build_dir}")
            
            # Start local HTTP server
            try:
                port = find_free_port()
                self.frontend_thread = threading.Thread(
                    target=start_frontend_server,
                    args=(build_dir, port),
                    daemon=True
                )
                self.frontend_thread.start()
                
                # Wait a bit for server to start
                time.sleep(0.5)
                
                app_url = f'http://127.0.0.1:{port}'
                print(f"[App] Using local HTTP server: {app_url}")
                
            except Exception as e:
                print(f"[App] [X] Failed to start frontend server: {e}")
                # Fallback to file:// if server fails (though likely to have CORS issues)
                import pathlib
                index_html = build_dir / 'index.html'
                app_url = index_html.resolve().as_uri()
                print(f"[App] Fallback to file URL: {app_url}")
        
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
        
        print("[App] [OK] Window created with React app")
    
    def start_sync_service(self):
        """Start the sync service if enabled"""
        if not ENABLE_AUTO_SYNC:
            print("[App] Auto-sync is disabled")
            return
        
        try:
            print("[App] Starting sync service...")
            self.sync_service = SyncService()
            self.sync_service.start()
            print("[App] [OK] Sync service started")
        except Exception as e:
            print(f"[App] Warning: Could not start sync service: {e}")
    
    def stop_sync_service(self):
        """Stop the sync service"""
        if self.sync_service:
            print("[App] Stopping sync service...")
            self.sync_service.stop()
            print("[App] Sync service stopped")

    def stop_backend(self):
        """Backend runs as a daemon thread — it stops automatically with the process"""
        print("[App] Backend thread will stop with the process")
    
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
            if FROZEN:
                # In frozen mode, show a message box with Hardware ID
                import ctypes
                hw_id_display = self.hardware_id or "(could not get)"
                msg = f"License validation failed.\\n\\nYour Hardware ID:\\n{hw_id_display}\\n\\nPlease send this ID to get a valid license."
                ctypes.windll.user32.MessageBoxW(0, msg, "SweetShopMa - License Required", 0x40)
            else:
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
            if FROZEN:
                import ctypes
                ctypes.windll.user32.MessageBoxW(0, "Django backend failed to start.", "SweetShopMa - Error", 0x10)
            else:
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
            # Stop backend on exit
            self.stop_backend()
            # Note: frontend thread is daemon, so it dies with main thread
        
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
    parser.add_argument(
        '--show-hwid',
        action='store_true',
        help='Show hardware ID and exit (for license generation)'
    )
    
    args = parser.parse_args()
    
    # Handle --show-hwid flag
    if args.show_hwid:
        hw_id = get_hardware_id()
        print(f"\nHardware ID: {hw_id}\n")
        if FROZEN:
            import ctypes
            ctypes.windll.user32.MessageBoxW(0, f"Hardware ID:\n\n{hw_id}\n\nCopy this ID and send it to get a license.", "SweetShopMa - Hardware ID", 0x40)
        sys.exit(0)
    
    # Create and start application
    app = Application(dev_mode=args.dev)
    app.start()


if __name__ == '__main__':
    main()
