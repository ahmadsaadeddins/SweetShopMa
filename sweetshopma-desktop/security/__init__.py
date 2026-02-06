"""
Security Module for SweetShopMa Desktop Application

This module provides:
- Hardware ID generation
- License management
- Encryption key management
- Anti-debugging protection
"""

from .hardware_id import get_hardware_id, display_hardware_id
from .license_manager import LicenseManager
from .key_manager import KeyManager
from .anti_debug import detect_debugger, enable_protections

__all__ = [
    'get_hardware_id',
    'display_hardware_id',
    'LicenseManager',
    'KeyManager',
    'detect_debugger',
    'enable_protections',
]
