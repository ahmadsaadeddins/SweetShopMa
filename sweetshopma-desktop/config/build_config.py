"""
Build Configuration for SweetShopMa Desktop Application

This file contains settings for building the executable with PyInstaller
and obfuscating code with PyArmor.
"""

import os

# PyInstaller Configuration
PYINSTALLER_SETTINGS = {
    # Application name
    'name': 'SweetShopMa',
    
    # Entry point
    'script': 'frontend/main.py',
    
    # Icon (optional)
    'icon': None,  # Path to .ico file if available
    
    # Console window
    'console': False,  # Set to True for debugging
    
    # One file mode
    'onefile': True,
    
    # Windowed mode
    'windowed': True,
    
    # UPX compression
    'upx': True,
    
    # Strip debug info
    'strip': False,
    
    # Additional data files
    'datas': [
        ('frontend/templates', 'templates'),
        ('frontend/static', 'static'),
        ('backend', 'backend'),
    ],
    
    # Hidden imports
    'hiddenimports': [
        'webview',
        'requests',
        'jinja2',
        'cryptography',
        'sqlcipher3',
        '_sqlite3',
        'django',
        'rest_framework',
        'security.hardware_id',
        'security.license_manager',
        'security.key_manager',
        'security.anti_debug',
        'sync.sync_service',
    ],
    
    # Excludes (to reduce size)
    'excludes': [
        'tkinter',
        'matplotlib',
        'numpy',
        'pandas',
        'scipy',
    ],
}

# PyArmor Configuration
PYARMOR_SETTINGS = {
    # Obfuscation mode
    'mode': 'normal',  # normal, advanced, super
    
    # Files to obfuscate
    'files': [
        'security/hardware_id.py',
        'security/license_manager.py',
        'security/key_manager.py',
        'security/anti_debug.py',
        'frontend/main.py',
        'frontend/api.py',
        'sync/sync_service.py',
    ],
    
    # Output directory
    'output': 'dist/obfuscated',
    
    # Recursive obfuscation
    'recursive': True,
    
    # Mix strategies
    'mix_str': True,
    'mix_class': True,
    'mix_func': True,
}

# Build Paths
BUILD_DIR = 'build'
DIST_DIR = 'dist'
SPEC_FILE = 'sweetshopma.spec'

# Output Files
OUTPUT_EXE = os.path.join(DIST_DIR, 'SweetShopMa.exe')
OUTPUT_DIR = DIST_DIR

# Build Settings
CLEAN_BEFORE_BUILD = True
RUN_AFTER_BUILD = False
CREATE_INSTALLER = False

# Version Info (for Windows executable)
VERSION_INFO = {
    'version': '1.0.0',
    'company': 'SweetShopMa',
    'product': 'SweetShopMa Desktop',
    'description': 'Point of Sale System for Sweet Shop',
    'copyright': 'Copyright 2024',
}
