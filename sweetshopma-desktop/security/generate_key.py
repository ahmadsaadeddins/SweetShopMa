"""
Generate Fernet Encryption Key

This script generates a secure Fernet encryption key and saves it to a config file.
Run this once to generate the key for the application.
"""

import os
import json
from pathlib import Path
from cryptography.fernet import Fernet


def generate_fernet_key():
    """Generate a new Fernet encryption key."""
    return Fernet.generate_key()


def save_key_to_config(key, config_path=None):
    """
    Save the encryption key to a config file.
    
    Args:
        key (bytes): The Fernet key to save
        config_path (str, optional): Path to config file. Defaults to security_config.json
    """
    if config_path is None:
        # Default to parent directory of security module
        config_path = Path(__file__).parent.parent / "security_config.json"
    else:
        config_path = Path(config_path)
    
    # Create config dictionary
    config = {
        "fernet_key": key.decode('utf-8'),
        "generated_at": str(Path(__file__).stat().st_mtime),
        "note": "Keep this file secure and never commit it to version control!"
    }
    
    # Save to file
    with open(config_path, 'w') as f:
        json.dump(config, f, indent=2)
    
    # Set file permissions (read/write for owner only on Unix-like systems)
    try:
        os.chmod(config_path, 0o600)
    except Exception:
        pass  # Windows doesn't support chmod in the same way
    
    print(f"[OK] Fernet key generated and saved to: {config_path}")
    print(f"[OK] Key length: {len(key)} bytes")
    print(f"[!] IMPORTANT: Keep this file secure and add it to .gitignore!")
    
    return config_path


def main():
    """Main function to generate and save the key."""
    print("=" * 60)
    print("Fernet Encryption Key Generator")
    print("=" * 60)
    print()
    
    # Generate key
    print("Generating new Fernet key...")
    key = generate_fernet_key()
    print(f"[OK] Key generated: {key.decode('utf-8')}")
    print()
    
    # Save to config
    config_path = save_key_to_config(key)
    print()
    
    # Verify the key is valid
    print("Verifying key...")
    try:
        cipher = Fernet(key)
        test_data = b"Test data for encryption"
        encrypted = cipher.encrypt(test_data)
        decrypted = cipher.decrypt(encrypted)
        assert decrypted == test_data
        print("[OK] Key verification successful!")
    except Exception as e:
        print(f"[ERROR] Key verification failed: {e}")
        return 1
    
    print()
    print("=" * 60)
    print("Key generation complete!")
    print("=" * 60)
    print()
    print("Next steps:")
    print("1. Add 'security_config.json' to your .gitignore file")
    print("2. Never commit this file to version control")
    print("3. Keep a backup of this key in a secure location")
    print("4. Run the application to test the configuration")
    
    return 0


if __name__ == "__main__":
    exit(main())
