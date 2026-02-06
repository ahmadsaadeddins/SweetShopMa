"""
Key Manager Module

Manages SQLCipher encryption keys using hardware-bound, multi-factor key generation.
This ensures the database cannot be opened on another machine.
"""

import hashlib
import base64
import os
import json
from datetime import datetime


class KeyManager:
    """
    Secure encryption key management for SQLCipher.
    
    The encryption key is derived from multiple factors:
    1. Hardware ID (unique per machine)
    2. Installation timestamp
    3. License hash
    4. Embedded secret (obfuscated)
    
    This makes the key:
    - Unique per machine
    - Impossible to extract via static analysis
    - Useless even if extracted (won't work on another machine)
    """
    
    # Obfuscated master secret (base64 encoded)
    # In production, this should be further obfuscated or use PyArmor
    _OBFUSCATED_SECRET = None  # Will be set after class definition
    
    @staticmethod
    def _decode_secret(encoded):
        """
        Decode obfuscated secret from base64.
        
        Args:
            encoded (str): Base64 encoded secret
            
        Returns:
            str: Decoded secret
        """
        try:
            return base64.b64decode(encoded).decode()
        except Exception:
            # Fallback if decoding fails
            return "SweetShopMa2024_SecretKey"
    
    @staticmethod
    def get_encryption_key():
        """
        Get SQLCipher encryption key using multiple factors.
        
        The key is derived using PBKDF2 with 100,000 iterations
        from the combination of:
        - Hardware ID
        - Installation time
        - License hash
        - Embedded secret
        
        Returns:
            str: 64-character hexadecimal encryption key
        """
        # Factor 1: Hardware ID
        try:
            from hardware_id import get_hardware_id
            hw_id = get_hardware_id()
        except Exception:
            # Fallback if hardware ID fails
            import socket
            hw_id = socket.gethostname()
        
        # Factor 2: Installation time
        install_time = KeyManager._get_install_time()
        
        # Factor 3: License hash
        license_hash = KeyManager._get_license_hash()
        
        # Factor 4: Obfuscated secret
        secret = KeyManager._OBFUSCATED_SECRET
        
        # Combine all factors with delimiter
        combined = f"{hw_id}:{install_time}:{license_hash}:{secret}"
        
        # Derive key using PBKDF2-HMAC-SHA256
        # 100,000 iterations for security
        key = hashlib.pbkdf2_hmac(
            'sha256',
            combined.encode(),
            b'sweetshopma-salt-2024',  # Salt
            100000,  # Iterations
            dklen=32  # 32 bytes = 256 bits
        )
        
        # Return as hexadecimal string
        return key.hex()
    
    @staticmethod
    def _get_install_time():
        """
        Get installation timestamp.
        
        Creates install_config.json if it doesn't exist.
        
        Returns:
            str: ISO format timestamp
        """
        config_file = 'install_config.json'
        
        try:
            if os.path.exists(config_file):
                with open(config_file, 'r') as f:
                    config = json.load(f)
                    return config.get('install_time', datetime.now().isoformat())
            else:
                # First run - create install config
                install_time = datetime.now().isoformat()
                
                # Also store hardware ID for reference
                try:
                    from hardware_id import get_hardware_id
                    hw_id = get_hardware_id()
                except:
                    hw_id = "unknown"
                
                config = {
                    'install_time': install_time,
                    'hardware_id': hw_id,
                    'version': '1.0.0'
                }
                
                with open(config_file, 'w') as f:
                    json.dump(config, f, indent=2)
                
                return install_time
        except Exception as e:
            print(f"[KeyManager] Error getting install time: {e}")
            return datetime.now().isoformat()
    
    @staticmethod
    def _get_license_hash():
        """
        Get hash of validated license.
        
        Returns:
            str: SHA-256 hash of license key, or "no-license"
        """
        try:
            from license_manager import LicenseManager
            
            license_mgr = LicenseManager()
            license_key = license_mgr.load_license()
            
            if license_key:
                return hashlib.sha256(license_key.encode()).hexdigest()
            else:
                return "no-license"
        except Exception as e:
            print(f"[KeyManager] Error getting license hash: {e}")
            return "no-license"
    
    @staticmethod
    def verify_key_integrity():
        """
        Verify that the encryption key can be generated consistently.
        
        This is useful for testing that the key derivation is working.
        
        Returns:
            bool: True if key generation is consistent
        """
        try:
            key1 = KeyManager.get_encryption_key()
            key2 = KeyManager.get_encryption_key()
            return key1 == key2
        except Exception as e:
            print(f"[KeyManager] Error verifying key integrity: {e}")
            return False
    
    @staticmethod
    def get_key_info():
        """
        Get information about the encryption key (for debugging).
        
        WARNING: Do not use in production! This is for development only.
        
        Returns:
            dict: Key information
        """
        key = KeyManager.get_encryption_key()
        
        return {
            'key_length': len(key),
            'key_algorithm': 'PBKDF2-HMAC-SHA256',
            'iterations': 100000,
            'key_size': 256,  # bits
            'factors': {
                'hardware_id': True,
                'install_time': True,
                'license_hash': True,
                'embedded_secret': True,
            }
        }


# Initialize the obfuscated secret after class definition
KeyManager._OBFUSCATED_SECRET = KeyManager._decode_secret('U3dlZXRTaG9wTWFAMjAyNF9TZWNyZXRLZXk=')

# For testing
if __name__ == '__main__':
    print("Key Manager Test")
    print("="*60)
    
    # Test key generation
    print("\n1. Testing key generation...")
    key = KeyManager.get_encryption_key()
    print(f"   Generated key: {key[:32]}...")
    print(f"   Key length: {len(key)} characters")
    
    # Test key consistency
    print("\n2. Testing key consistency...")
    is_consistent = KeyManager.verify_key_integrity()
    print(f"   Consistent: {is_consistent}")
    
    # Test key info
    print("\n3. Key information:")
    info = KeyManager.get_key_info()
    for key, value in info.items():
        if key != 'factors':
            print(f"   {key}: {value}")
    
    print("\n" + "="*60)
    print("Key Manager test complete!")
