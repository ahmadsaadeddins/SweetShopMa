"""
Configuration module for SweetShopMa Desktop Application.
"""

from .settings import *
from .build_config import *

__all__ = [
    'APP_NAME',
    'APP_VERSION',
    'APP_VERSION_DISPLAY',
    'BRANCH_ID',
    'BRANCH_NAME',
    'CENTRAL_SERVER_URL',
    'SYNC_INTERVAL',
    'DB_NAME',
    'LICENSE_FILE',
    'PYINSTALLER_SETTINGS',
    'PYARMOR_SETTINGS',
]
