#!/usr/bin/env python
"""Generate license for the current machine"""

import sys
sys.path.insert(0, '.')

from security.license_manager import LicenseManager
from security.hardware_id import get_hardware_id

# Get current hardware ID
hw_id = get_hardware_id()
print(f"Hardware ID: {hw_id}")

# Generate license
lm = LicenseManager()
license_key = lm.generate_license(
    hardware_id=hw_id,
    expiry_days=365,
    licensee_name="Test User",
    license_type="standard"
)

# Save to dist folder
lm.save_license(license_key, 'dist/license.key')
print("License saved to dist/license.key")

# Verify
is_valid, message, info = lm.validate_license(license_key, hw_id)
print(f"Validation: {is_valid} - {message}")
