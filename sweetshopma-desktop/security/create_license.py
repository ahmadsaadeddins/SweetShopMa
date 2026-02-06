"""
License Generator for SweetShopMa Desktop

This script generates a license file for a specific hardware ID.
Run this after getting the hardware ID from the application.
"""

import sys
from pathlib import Path

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))

from security.license_manager import LicenseManager


def generate_license_file(hardware_id, output_path=None):
    """
    Generate a license file for the given hardware ID.
    
    Args:
        hardware_id (str): The hardware ID to license
        output_path (str, optional): Path to save the license file
    """
    if output_path is None:
        output_path = Path(__file__).parent.parent / "license.key"
    else:
        output_path = Path(output_path)
    
    # Create license manager
    license_mgr = LicenseManager()
    
    # Generate license (perpetual, for testing)
    license_data = license_mgr.generate_license(
        hardware_id=hardware_id,
        expiry_days=None,  # None = perpetual
        licensee_name="Test License",
        license_type="standard",
        features=["all"]
    )
    
    # Save license to file
    with open(output_path, 'w') as f:
        f.write(license_data)
    
    print(f"[OK] License file created: {output_path}")
    print(f"[OK] Hardware ID: {hardware_id}")
    print(f"[OK] License Type: Standard (Perpetual)")
    print(f"[OK] Features: All")
    
    return output_path


def main():
    """Main function."""
    print("=" * 60)
    print("SweetShopMa License Generator")
    print("=" * 60)
    print()
    
    # Get hardware ID from command line or prompt
    if len(sys.argv) > 1:
        hardware_id = sys.argv[1]
    else:
        hardware_id = input("Enter Hardware ID: ").strip()
    
    if not hardware_id:
        print("[ERROR] Hardware ID is required!")
        return 1
    
    print()
    print("Generating license...")
    
    try:
        generate_license_file(hardware_id)
        print()
        print("=" * 60)
        print("License generation complete!")
        print("=" * 60)
        print()
        print("Next steps:")
        print("1. Copy the license.key file to the application directory")
        print("2. Restart the application")
        print("3. The application should now start successfully")
        return 0
    except Exception as e:
        print(f"[ERROR] Failed to generate license: {e}")
        import traceback
        traceback.print_exc()
        return 1


if __name__ == "__main__":
    exit(main())
