"""
License Manager Module

Handles license generation, validation, and management.
Uses Fernet symmetric encryption for secure license storage.
"""

import json
import os
import base64
from datetime import datetime, timedelta
from pathlib import Path
from cryptography.fernet import Fernet, InvalidToken
import hashlib


class LicenseManager:
    """
    Manages license generation, validation, and storage.
    
    Licenses are encrypted using Fernet symmetric encryption
    and contain hardware ID, licensee information, and expiry date.
    """
    
    @staticmethod
    def _load_encryption_key():
        """
        Load encryption key from config file.
        
        Returns:
            bytes: The Fernet encryption key
            
        Raises:
            FileNotFoundError: If config file doesn't exist
            ValueError: If key is invalid
        """
        # Try to load from config file
        config_path = Path(__file__).parent.parent / "security_config.json"
        
        if config_path.exists():
            try:
                with open(config_path, 'r') as f:
                    config = json.load(f)
                
                key = config.get('fernet_key')
                if key:
                    return key.encode('utf-8')
            except Exception as e:
                print(f"Warning: Failed to load key from config: {e}")
        
        # Fallback: Generate a temporary key (NOT RECOMMENDED FOR PRODUCTION)
        print("Warning: Using temporary key. Run 'python security/generate_key.py' to generate a permanent key.")
        return Fernet.generate_key()
    
    def __init__(self, secret_key=None):
        """
        Initialize LicenseManager with encryption key.
        
        Args:
            secret_key (bytes, optional): 32-byte URL-safe base64-encoded key.
                                         If None, loads from config file or generates temporary key.
        """
        if secret_key is None:
            secret_key = self._load_encryption_key()
        
        # Validate key format
        if not isinstance(secret_key, bytes):
            raise ValueError(f"Secret key must be bytes, got {type(secret_key)}")
        
        if len(secret_key) != 44:  # 32 bytes base64-encoded = 44 characters
            raise ValueError(f"Secret key must be 44 characters (32 bytes base64-encoded), got {len(secret_key)}")
        
        self.cipher = Fernet(secret_key)
    
    def generate_license(self, hardware_id, expiry_days=None, licensee_name="", 
                        license_type="standard", features=None):
        """
        Generate a license for a specific hardware ID.
        
        Args:
            hardware_id (str): The hardware ID to license
            expiry_days (int, optional): Number of days until expiry. None = perpetual
            licensee_name (str): Name of the licensee (company/person)
            license_type (str): Type of license (standard, trial, enterprise)
            features (list, optional): List of enabled features
            
        Returns:
            str: Encrypted license key (base64 encoded)
        """
        # Calculate expiry date
        expiry_date = None
        if expiry_days:
            expiry_date = (datetime.now() + timedelta(days=expiry_days)).isoformat()
        
        # Create license data
        license_data = {
            'hardware_id': hardware_id,
            'licensee': licensee_name,
            'license_type': license_type,
            'issued_date': datetime.now().isoformat(),
            'expiry_date': expiry_date,
            'version': '1.0',
            'features': features or ['all'],
        }
        
        # Add checksum for tamper detection
        license_data['checksum'] = self._calculate_checksum(license_data)
        
        # Encrypt the license data
        json_data = json.dumps(license_data, sort_keys=True)
        encrypted_data = self.cipher.encrypt(json_data.encode())
        
        # Encode to base64 for file storage
        license_key = base64.b64encode(encrypted_data).decode()
        
        return license_key
    
    def validate_license(self, license_key, current_hardware_id):
        """
        Validate a license key against the current hardware ID.
        
        Args:
            license_key (str): The license key to validate
            current_hardware_id (str): The current machine's hardware ID
            
        Returns:
            tuple: (is_valid, message, license_info)
                - is_valid (bool): True if license is valid
                - message (str): Validation message
                - license_info (dict or None): License data if valid
        """
        try:
            # Decrypt the license key
            encrypted_data = base64.b64decode(license_key.encode())
            decrypted_data = self.cipher.decrypt(encrypted_data)
            license_data = json.loads(decrypted_data.decode())
            
            # Verify checksum
            stored_checksum = license_data.pop('checksum', None)
            calculated_checksum = self._calculate_checksum(license_data)
            
            if stored_checksum != calculated_checksum:
                return False, "License has been tampered with", None
            
            # Restore checksum for return value
            license_data['checksum'] = stored_checksum
            
            # Check if hardware ID matches
            if license_data['hardware_id'] != current_hardware_id:
                return False, "License is not valid for this machine", None
            
            # Check if license has expired
            if license_data.get('expiry_date'):
                try:
                    expiry_date = datetime.fromisoformat(license_data['expiry_date'])
                    if datetime.now() > expiry_date:
                        return False, f"License expired on {expiry_date.strftime('%Y-%m-%d')}", None
                except ValueError:
                    return False, "Invalid expiry date in license", None
            
            # License is valid
            licensee = license_data.get('licensee', 'Unknown')
            message = f"License valid for {licensee}"
            
            return True, message, license_data
            
        except InvalidToken:
            return False, "Invalid license file (decryption failed)", None
        except json.JSONDecodeError:
            return False, "Invalid license file (corrupted data)", None
        except Exception as e:
            return False, f"Error validating license: {str(e)}", None
    
    def save_license(self, license_key, filepath='license.key'):
        """
        Save license key to file.
        
        Args:
            license_key (str): The license key to save
            filepath (str): Path to save the license file
        """
        try:
            with open(filepath, 'w') as f:
                f.write(license_key)
            print(f"[License] License saved to {filepath}")
        except Exception as e:
            print(f"[License] Error saving license: {e}")
            raise
    
    def load_license(self, filepath=None):
        """
        Load license key from file.
        
        Args:
            filepath (str, optional): Path to the license file.
                                     If None, looks in sweetshopma-desktop directory.
            
        Returns:
            str or None: The license key, or None if file doesn't exist
        """
        try:
            if filepath is None:
                # Default to license.key in sweetshopma-desktop directory
                filepath = Path(__file__).parent.parent / "license.key"
            else:
                filepath = Path(filepath)
            
            if not filepath.exists():
                return None
            
            with open(filepath, 'r') as f:
                license_key = f.read().strip()
            
            return license_key if license_key else None
        except Exception as e:
            print(f"[License] Error loading license: {e}")
            return None
    
    def _calculate_checksum(self, license_data):
        """
        Calculate checksum for license data (tamper detection).
        
        Args:
            license_data (dict): License data
            
        Returns:
            str: Hexadecimal checksum
        """
        # Create string from license data (excluding checksum field)
        data_str = json.dumps(license_data, sort_keys=True)
        return hashlib.sha256(data_str.encode()).hexdigest()
    
    def get_license_info(self, license_key):
        """
        Get license information without validation.
        Useful for displaying license details.
        
        Args:
            license_key (str): The license key
            
        Returns:
            dict or None: License information, or None if invalid
        """
        try:
            encrypted_data = base64.b64decode(license_key.encode())
            decrypted_data = self.cipher.decrypt(encrypted_data)
            license_data = json.loads(decrypted_data.decode())
            return license_data
        except:
            return None


# For testing
if __name__ == '__main__':
    # Test license generation and validation
    manager = LicenseManager()
    
    # Generate a test license
    test_hw_id = "a" * 64  # Test hardware ID
    license_key = manager.generate_license(
        hardware_id=test_hw_id,
        expiry_days=365,
        licensee_name="Test Company",
        license_type="standard"
    )
    
    print("Generated License Key:")
    print(license_key[:100] + "...")
    print()
    
    # Validate the license
    is_valid, message, info = manager.validate_license(license_key, test_hw_id)
    print(f"Validation: {is_valid}")
    print(f"Message: {message}")
    print(f"Info: {info}")
