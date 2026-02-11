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
        self.backend_process = None  # Backend EXE process
        self.django_ready = threading.Event()
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
    
    def start_django(self):
        """Start Django backend by launching the backend EXE"""
        print("[App] Starting Django backend...")
        
        # Find backend executable or script
        if not FROZEN:
            # In development/source mode, run the python script
            backend_script = EXE_DIR / 'backend_main.py'
            if not backend_script.exists():
                print(f"[App] [X] Backend script not found: {backend_script}")
                return False
            
            cmd = [sys.executable, str(backend_script)]
            print(f"[App] Backend script: {backend_script}")
        else:
            # In frozen mode, run the compiled EXE
            backend_exe = EXE_DIR / 'SweetShopMa_Backend.exe'
            if not backend_exe.exists():
                print(f"[App] [X] Backend EXE not found: {backend_exe}")
                return False
            
            cmd = [str(backend_exe)]
            print(f"[App] Backend EXE: {backend_exe}")
        
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
        
        # Launch backend
        try:
            import subprocess
            self.backend_process = subprocess.Popen(
                cmd,
                cwd=str(EXE_DIR),
                creationflags=subprocess.CREATE_NEW_CONSOLE if not FROZEN else 0
            )
            print(f"[App] Backend process started (PID: {self.backend_process.pid})")
        except Exception as e:
            print(f"[App] [X] Failed to start backend: {e}")
            return False
        
        # Wait for Django to be ready
        print("[App] Waiting for Django to start...")
        for i in range(30):  # Wait up to 30 seconds
            try:
                import requests
                # Try root URL first (simpler check)
                response = requests.get('http://127.0.0.1:8000/', timeout=1)
                print(f"[App] Health check attempt {i+1}: Status {response.status_code}")
                if response.status_code in [200, 404]:  # 404 is ok - means server is running
                    print("[App] [OK] Django is ready")
                    self.django_ready.set()
                    return True
            except requests.exceptions.ConnectionError as e:
                print(f"[App] Attempt {i+1}: Connection error - waiting...")
                time.sleep(1)
            except requests.exceptions.Timeout as e:
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
        """Stop the backend process"""
        if self.backend_process:
            print(f"[App] Stopping backend process (PID: {self.backend_process.pid})...")
            try:
                # terminate() is gentler than kill()
                self.backend_process.terminate()
                try:
                    self.backend_process.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    print("[App] Backend did not stop, killing...")
                    self.backend_process.kill()
                print("[App] Backend process stopped")
            except Exception as e:
                print(f"[App] Error stopping backend: {e}")
            self.backend_process = None
    
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
