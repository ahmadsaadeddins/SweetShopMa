"""
Clock Guard Module

Prevents users from bypassing trial expiration by manipulating the system clock.
Implements a 3-layer defense strategy:
1. Usage Day Counting (Registry + Encrypted File)
2. Last Run Timestamp Check
3. NTP Time Verification (Online)
"""

import os
import json
import time
import socket
import struct
import hashlib
import platform
import winreg
from datetime import datetime, timedelta
from pathlib import Path
from cryptography.fernet import Fernet

class ClockGuard:
    """
    Guard against clock tampering for trial licenses.
    Stores and verifies timestamp data in both local file and Windows Registry.
    """
    
    # NTP server to check against
    NTP_SERVER = "pool.ntp.org"
    
    # Registry path for backup storage
    REG_PATH = r"Software\SweetShopMa\Security"
    
    # File name for local storage (hidden)
    # We use a generic name to avoid suspicion
    STORAGE_FILE = ".scheck"
    
    def __init__(self, fernet_key):
        """
        Initialize ClockGuard.
        
        Args:
            fernet_key (bytes): Fernet encryption key
        """
        self.cipher = Fernet(fernet_key)
        
        # Determine storage path
        # If frozen, store next to EXE. If dev, store in project root.
        import sys
        if getattr(sys, 'frozen', False):
            self.base_dir = Path(sys.executable).parent
        else:
            self.base_dir = Path(__file__).parent.parent
            
        self.storage_path = self.base_dir / self.STORAGE_FILE

    def check_tamper(self, max_trial_days):
        """
        Check for clock tampering or trial expiration.
        
        Args:
            max_trial_days (int): Maximum allowed usage days for trial
            
        Returns:
            tuple: (is_ok, reason)
                - is_ok (bool): True if check passed
                - reason (str): Failure reason if any
        """
        # 1. Load state from File and Registry (reconcile them)
        state = self._reconcile_state()
        
        if not state:
            # First run or state lost - initialize empty state
            state = {
                'first_run': datetime.utcnow().isoformat(),
                'last_run': datetime.utcnow().isoformat(),
                'usage_days': [],  # List of unique date hashes
                'tamper_count': 0
            }
            # We don't save yet, wait for record_launch
        
        # 2. Check Layer 3: NTP Time (if online)
        # This prevents "Run with clock set to 2020" attack
        ntp_time = self._get_ntp_time()
        if ntp_time:
            # Check if system time is significantly behind NTP time (> 2 hours)
            # We use a large buffer to account for time zones / minor drift
            system_time = datetime.utcnow()
            time_diff = ntp_time - system_time
            
            if abs(time_diff.total_seconds()) > 7200:  # 2 hours
                return False, "System clock is incorrect (sync with internet required)"
            
            # Also check if NTP time is > last_run (if we have history)
            last_run = datetime.fromisoformat(state['last_run'])
            if ntp_time < last_run - timedelta(hours=2):
                return False, "System clock appears to have been rolled back (NTP check)"

        # 3. Check Layer 2: Last Run Timestamp
        # This prevents "Run, close, set clock back 1 day, run again"
        current_time = datetime.utcnow()
        last_run = datetime.fromisoformat(state['last_run'])
        
        # Allow 1 hour backward drift (e.g. time correction), but not more
        if current_time < last_run - timedelta(hours=1):
            return False, "System clock has been set back"
            
        # 4. Check Layer 1: Usage Days (The Strongest Check)
        # We count legitimate unique days the app was used.
        # Even if user sets clock back, we already have N days recorded.
        # They can't un-use a day.
        
        # Count distinct days
        unique_days = len(state.get('usage_days', []))
        
        # If we are adding a NEW day today, checking it now
        today_hash = self._get_date_hash(current_time)
        if today_hash not in state.get('usage_days', []):
            unique_days += 1
            
        if unique_days > max_trial_days:
            return False, f"Trial expired ({unique_days}/{max_trial_days} days used)"
            
        return True, None

    def record_launch(self):
        """
        Record a successful launch.
        Updates usage days and last run timestamp.
        Persists to both File and Registry.
        """
        # Load existing state
        state = self._reconcile_state()
        
        current_time = datetime.utcnow()
        
        if not state:
             state = {
                'first_run': current_time.isoformat(),
                'last_run': current_time.isoformat(),
                'usage_days': [],
                'tamper_count': 0
            }
            
        # Update last run
        state['last_run'] = current_time.isoformat()
        
        # Update usage days
        today_hash = self._get_date_hash(current_time)
        if 'usage_days' not in state:
            state['usage_days'] = []
            
        if today_hash not in state['usage_days']:
            state['usage_days'].append(today_hash)
            
        # Save to both locations
        self._save_state(state)

    def _get_date_hash(self, date_obj):
        """
        Create a hash for a specific date (YYYY-MM-DD).
        """
        date_str = date_obj.strftime("%Y-%m-%d")
        # Add a salt to prevent rainbow table attacks
        salt = "SweetShopMa_Salt_998877" 
        return hashlib.sha256((date_str + salt).encode()).hexdigest()

    def _reconcile_state(self):
        """
        Load state from both File and Registry.
        Verify integrity and return the 'most advanced' state (highest usage).
        This prevents deletion attacks (user deletes file, registry restores it).
        """
        file_state = self._load_from_file()
        reg_state = self._load_from_registry()
        
        if not file_state and not reg_state:
            return None
            
        if not file_state:
            # File deleted, restore from registry
            self._save_to_file(reg_state)
            return reg_state
            
        if not reg_state:
            # Registry deleted, restore from file
            self._save_to_registry(file_state)
            return file_state
            
        # Both exist - compare usage
        # We trust the one with MORE usage days or LATER timestamp
        file_days = len(file_state.get('usage_days', []))
        reg_days = len(reg_state.get('usage_days', []))
        
        if file_days > reg_days:
            # File is ahead, update registry
            self._save_to_registry(file_state)
            return file_state
        elif reg_days > file_days:
            # Registry is ahead, update file
            self._save_to_file(reg_state)
            return reg_state
        else:
            # Days equal, check timestamp
            try:
                file_time = datetime.fromisoformat(file_state['last_run'])
                reg_time = datetime.fromisoformat(reg_state['last_run'])
                
                if file_time > reg_time:
                    self._save_to_registry(file_state)
                    return file_state
                else:
                    self._save_to_file(reg_state)
                    return reg_state
            except:
                return file_state

    def _load_from_file(self):
        """Load encrypted state from hidden file."""
        if not self.storage_path.exists():
            return None
            
        try:
            with open(self.storage_path, 'rb') as f:
                encrypted_data = f.read()
                
            json_data = self.cipher.decrypt(encrypted_data).decode()
            return json.loads(json_data)
        except Exception as e:
            # If file is corrupted or tampered, we return None
            # The Registry backup will hopefully take over
            print(f"[ClockGuard] Error loading file: {e}")
            return None

    def _save_to_file(self, state):
        """Save state to encrypted file."""
        try:
            json_data = json.dumps(state)
            encrypted_data = self.cipher.encrypt(json_data.encode())
            
            # On Windows, we need to remove HIDDEN attribute before writing
            if platform.system() == 'Windows' and self.storage_path.exists():
                import ctypes
                FILE_ATTRIBUTE_NORMAL = 0x80
                ctypes.windll.kernel32.SetFileAttributesW(str(self.storage_path), FILE_ATTRIBUTE_NORMAL)
            
            with open(self.storage_path, 'wb') as f:
                f.write(encrypted_data)
                
            # Re-apply hidden attribute on Windows
            if platform.system() == 'Windows':
                import ctypes
                FILE_ATTRIBUTE_HIDDEN = 0x02
                ctypes.windll.kernel32.SetFileAttributesW(str(self.storage_path), FILE_ATTRIBUTE_HIDDEN)
        except Exception as e:
            print(f"[ClockGuard] Error saving file: {e}")

    def _load_from_registry(self):
        """Load state from Windows Registry."""
        if platform.system() != 'Windows':
            return None
            
        try:
            key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, self.REG_PATH, 0, winreg.KEY_READ)
            encrypted_data, _ = winreg.QueryValueEx(key, "Data")
            winreg.CloseKey(key)
            
            # Registry data is bytes, decrypt it
            json_data = self.cipher.decrypt(encrypted_data).decode()
            return json.loads(json_data)
        except WindowsError:
            # Key not found
            return None
        except Exception as e:
            print(f"[ClockGuard] Error loading registry: {e}")
            return None

    def _save_to_registry(self, state):
        """Save state to Windows Registry."""
        if platform.system() != 'Windows':
            return
            
        try:
            # Create key if not exists
            key = winreg.CreateKey(winreg.HKEY_CURRENT_USER, self.REG_PATH)
            
            json_data = json.dumps(state)
            encrypted_data = self.cipher.encrypt(json_data.encode())
            
            winreg.SetValueEx(key, "Data", 0, winreg.REG_BINARY, encrypted_data)
            winreg.CloseKey(key)
        except Exception as e:
            print(f"[ClockGuard] Error saving registry: {e}")

    def _save_state(self, state):
        """Save to both locations."""
        self._save_to_file(state)
        self._save_to_registry(state)

    def _get_ntp_time(self):
        """
        Query NTP server for current time.
        Returns datetime object (UTC) or None if offline/failed.
        """
        # Simple SNTP client implementation
        # RFC 2030
        try:
            client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            client.settimeout(2.0)  # Short timeout
            
            # Message: 00 011 011 (LI, VN, Mode) + 47 bytes of 0
            msg = '\x1b' + 47 * '\0'
            
            client.sendto(msg.encode('utf-8'), (self.NTP_SERVER, 123))
            data, address = client.recvfrom(1024)
            
            if data:
                # Unpack 'Transmit Timestamp' (40-48 bytes)
                # Timestamp is seconds since 1900-01-01
                t = struct.unpack('!12I', data)[10]
                t -= 2208988800  # Convert 1900 epoch to 1970 epoch
                
                return datetime.utcfromtimestamp(t)
        except:
            # Return None on any error (offline, timeout, blocked)
            return None
        finally:
            client.close()
            
        return None
