#!/usr/bin/env python
"""
SweetShopMa Desktop - One-Click Build Script

Automates the build process:
1. Clean previous builds (optional)
2. Build React frontend (npm run build)
3. Build EXE(s) (PyInstaller)
   - Default: Combined Single EXE (SweetShopMa.exe)
   - With --split: Two EXEs (SweetShopMa.exe + SweetShopMa_Backend.exe)
4. Package for distribution

Usage:
    python scripts/build.py          # Full build (Single EXE)
    python scripts/build.py --split  # Build as split EXEs
    python scripts/build.py --fast   # Skip npm build and clean
"""

import os
import sys
import shutil
import subprocess
import time
import argparse
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
    print_step("Step", "Cleaning previous build artifacts...")
    
    # Clean PyInstaller build/work dirs
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

def build_npm():
    """Build React frontend"""
    print_step("Step", "Building React Frontend...")
    
    # Check if node_modules exists
    if not (FRONTEND_DIR / 'node_modules').exists():
        print("   Installing dependencies...")
        if not run_command('npm install', cwd=FRONTEND_DIR):
            return False
            
    print("   Compiling React app...")
    return run_command('npm run build', cwd=FRONTEND_DIR)

def build_combined_exe():
    """Build single combined EXE using SweetShopMa_Combined.spec"""
    print_step("Step", "Building Combined EXE (Frontend + Backend)...")
    
    cmd = [
        sys.executable, '-m', 'PyInstaller',
        '--noconfirm',
        '--clean',
        '--distpath', str(DIST_DIR),
        '--workpath', str(BUILD_DIR),
        'SweetShopMa_Combined.spec'
    ]
    
    return run_command(' '.join(cmd))

def build_split_exes():
    """Build separate Frontend and Backend EXEs"""
    print_step("Step", "Building Split EXEs (1/2: Backend)...")
    
    # 1. Backend
    cmd_backend = [
        sys.executable, '-m', 'PyInstaller',
        '--noconfirm',
        '--clean',
        '--distpath', str(DIST_DIR),
        '--workpath', str(BUILD_DIR),
        'backend.spec'
    ]
    if not run_command(' '.join(cmd_backend)):
        return False

    print_step("Step", "Building Split EXEs (2/2: Frontend)...")

    # 2. Frontend
    cmd_frontend = [
        sys.executable, '-m', 'PyInstaller',
        '--noconfirm',
        '--clean',
        '--distpath', str(DIST_DIR),
        '--workpath', str(BUILD_DIR),
        'SweetShopMa.spec'
    ]
    return run_command(' '.join(cmd_frontend))

def create_distribution_files(is_split):
    """Create README and other files"""
    print_step("Step", "Finalizing Distribution...")
    
    readme_content = f"""SweetShopMa Desktop Application
===============================

How to Run
----------
1. Double-click "SweetShopMa.exe" to start the application.
{"2. The backend server starts automatically." if not is_split else "2. The application will verify SweetShopMa_Backend.exe is present."}
3. On first run, the app will:
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
"""
    
    readme_path = DIST_DIR / 'README_DIST.txt'
    with open(readme_path, 'w') as f:
        f.write(readme_content)
    print(f"   Created {readme_path}")
    
    # Check output
    frontend_exe = DIST_DIR / 'SweetShopMa.exe'
    
    if frontend_exe.exists():
        print(f"\n   [SUCCESS] Build complete: {DIST_DIR}")
        if is_split:
            print(f"             SweetShopMa.exe (Frontend)")
            if (DIST_DIR / 'SweetShopMa_Backend.exe').exists():
                 print(f"             SweetShopMa_Backend.exe (Backend)")
            else:
                 print(f"   [WARNING] Backend EXE missing!")
        else:
            print(f"             SweetShopMa.exe (Combined Frontend + Backend)")
    else:
        print("\n   [ERROR] SweetShopMa.exe not found in dist folder!")

def main():
    start_time = time.time()
    
    parser = argparse.ArgumentParser(description="Build SweetShopMa Desktop")
    parser.add_argument('--split', action='store_true', help="Build separate Frontend and Backend EXEs")
    parser.add_argument('--fast', action='store_true', help="Skip npm build and clean step")
    args = parser.parse_args()
    
    print("=" * 60)
    print(f"  {APP_NAME} Build System")
    print(f"  Mode: {'Split EXEs' if args.split else 'Single Combined EXE'}")
    print("=" * 60)
    
    if not args.fast:
        clean_build()
        if not build_npm():
            print("\n[FAILURE] Frontend build failed.")
            sys.exit(1)
    else:
        print("\n[FAST MODE] Skipping clean and npm build")
        
    if args.split:
        if not build_split_exes():
             print("\n[FAILURE] Split EXE build failed.")
             sys.exit(1)
    else:
        if not build_combined_exe():
            print("\n[FAILURE] Combined EXE build failed.")
            sys.exit(1)
        
    create_distribution_files(is_split=args.split)
    
    elapsed = time.time() - start_time
    print(f"\nTotal build time: {elapsed:.1f}s")

if __name__ == '__main__':
    main()
