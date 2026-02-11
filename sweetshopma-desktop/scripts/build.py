#!/usr/bin/env python
"""
SweetShopMa Desktop - One-Click Build Script

Automates the entire build process:
1. Clean previous builds (optional)
2. Build React frontend (npm run build)
3. Build Backend EXE (PyInstaller)
4. Build Frontend EXE (PyInstaller)
5. Package for distribution

Usage:
    python scripts/build.py          # Full build
    python scripts/build.py --fast   # Skip npm build and clean
"""

import os
import sys
import shutil
import subprocess
import time
from pathlib import Path

# Paths
SCRIPT_DIR = Path(__file__).parent
PROJECT_DIR = SCRIPT_DIR.parent
FRONTEND_DIR = PROJECT_DIR / 'frontend'
DIST_DIR = PROJECT_DIR / 'dist'
BUILD_DIR = PROJECT_DIR / 'build_temp'

# Application info
APP_NAME = 'SweetShopMa'

def print_step(step, msg):
    print(f"\n[{step}] {msg}")
    print("-" * 60)

def run_command(cmd, cwd=None, shell=True):
    """Run a shell command and check for errors"""
    print(f"   > {cmd}")
    try:
        subprocess.run(
            cmd,
            cwd=cwd or PROJECT_DIR,
            shell=shell,
            check=True
        )
        return True
    except subprocess.CalledProcessError as e:
        print(f"   [ERROR] Command failed: {e}")
        return False

def clean_build():
    """Clean build directories"""
    print_step("1/5", "Cleaning previous build artifacts...")
    
    # Clean PyInstaller build/work dirs (but keep dist for now to be safe with open files)
    if BUILD_DIR.exists():
        try:
            shutil.rmtree(BUILD_DIR)
            print(f"   Removed {BUILD_DIR}")
        except Exception as e:
            print(f"   [WARNING] Could not remove build dir: {e}")

    # Remove previous EXEs from dist but keep license/env
    if DIST_DIR.exists():
        for item in DIST_DIR.glob("*.exe"):
            try:
                item.unlink()
                print(f"   Removed {item.name}")
            except Exception as e:
                print(f"   [WARNING] Could not remove {item.name}: {e}")
                
        # Remove DB to ensure fresh migrations
        db_path = DIST_DIR / "sweetshopma.db"
        if db_path.exists():
            try:
                db_path.unlink()
                print(f"   Removed {db_path.name} (forcing fresh migration)")
            except:
                pass

def build_frontend():
    """Build React frontend"""
    print_step("2/5", "Building React Frontend...")
    
    # Check if node_modules exists
    if not (FRONTEND_DIR / 'node_modules').exists():
        print("   Installing dependencies...")
        if not run_command('npm install', cwd=FRONTEND_DIR):
            return False
            
    print("   Compiling React app...")
    return run_command('npm run build', cwd=FRONTEND_DIR)

def build_backend_exe():
    """Build Backend EXE using backend.spec"""
    print_step("3/5", "Building Backend EXE (Django)...")
    
    cmd = [
        sys.executable, '-m', 'PyInstaller',
        '--noconfirm',
        '--clean',
        '--distpath', str(DIST_DIR),
        '--workpath', str(BUILD_DIR),
        'backend.spec'
    ]
    
    # Convert list to string for run_command to display it nicely, 
    # but subprocess.run needs list if shell=False, or string if shell=True.
    # Our run_command uses shell=True so we join it.
    return run_command(' '.join(cmd))

def build_frontend_exe():
    """Build Frontend EXE using SweetShopMa.spec"""
    print_step("4/5", "Building Frontend EXE (PyWebView)...")
    
    cmd = [
        sys.executable, '-m', 'PyInstaller',
        '--noconfirm',
        '--clean',
        '--distpath', str(DIST_DIR),
        '--workpath', str(BUILD_DIR),
        'SweetShopMa.spec'
    ]
    
    return run_command(' '.join(cmd))

def create_distribution_files():
    """Create README and other files"""
    print_step("5/5", "Finalizing Distribution...")
    
    readme_content = f"""SweetShopMa Desktop Application
===============================

How to Run
----------
1. Double-click "SweetShopMa.exe" to start the application.
2. This will automatically launch the backend server ("SweetShopMa_Backend.exe").
3. On first run, the backend will:
   - Create the database (sweetshopma.db)
   - Apply all migrations
   - Create a default admin user (admin / admin)

Licensing
---------
The application requires a valid "license.key" file in the same folder.
To generate a license:
1. Run the application and note the Hardware ID.
2. Use the "generate_license.py" script with the Hardware ID.
3. Place the "license.key" file next to "SweetShopMa.exe".

Troubleshooting
---------------
- If errors occur, delete "sweetshopma.db" to reset the database.
- Check "backend_error.log".
"""
    
    readme_path = DIST_DIR / 'README_DIST.txt'
    with open(readme_path, 'w') as f:
        f.write(readme_content)
    print(f"   Created {readme_path}")
    
    # Check output
    if (DIST_DIR / 'SweetShopMa.exe').exists() and (DIST_DIR / 'SweetShopMa_Backend.exe').exists():
        print(f"\n   [SUCCESS] Build complete: {DIST_DIR}")
        print(f"             SweetShopMa.exe (Frontend)")
        print(f"             SweetShopMa_Backend.exe (Backend)")
    else:
        print("\n   [ERROR] Missing executable(s) in dist folder!")

def main():
    start_time = time.time()
    
    fast_mode = '--fast' in sys.argv
    
    print("=" * 60)
    print(f"  {APP_NAME} Build System")
    print("=" * 60)
    
    if not fast_mode:
        clean_build()
        if not build_frontend():
            print("\n[FAILURE] Frontend build failed.")
            sys.exit(1)
    else:
        print("\n[FAST MODE] Skipping clean and npm build")
        
    if not build_backend_exe():
        print("\n[FAILURE] Backend build failed.")
        sys.exit(1)
        
    if not build_frontend_exe():
        print("\n[FAILURE] Frontend build failed.")
        sys.exit(1)
        
    create_distribution_files()
    
    elapsed = time.time() - start_time
    print(f"\nTotal build time: {elapsed:.1f}s")

if __name__ == '__main__':
    main()
