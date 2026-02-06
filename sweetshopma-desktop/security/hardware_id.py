"""
Hardware ID Generation Module

Generates a unique hardware ID based on multiple machine identifiers.
This ID is used for hardware-based licensing.
"""

import uuid
import subprocess
import hashlib
import platform


def get_hardware_id():
    """
    Generate unique hardware ID based on multiple machine identifiers.
    
    This function collects:
    1. Machine GUID (Windows) or equivalent
    2. Motherboard serial number
    3. MAC address
    4. CPU ID
    
    Returns:
        str: 64-character hexadecimal string (SHA-256 hash)
    """
    identifiers = []
    
    # 1. Machine GUID (Windows)
    try:
        if platform.system() == 'Windows':
            result = subprocess.check_output(
                'wmic csproduct get uuid',
                shell=True
            ).decode()
            uuid_str = result.split('\n')[1].strip()
            if uuid_str:
                identifiers.append(uuid_str)
    except Exception as e:
        print(f"[Hardware ID] Failed to get machine GUID: {e}")
    
    # 2. Motherboard Serial Number
    try:
        if platform.system() == 'Windows':
            result = subprocess.check_output(
                'wmic baseboard get serialnumber',
                shell=True
            ).decode()
            serial = result.split('\n')[1].strip()
            # Skip generic values
            if serial and serial not in ['To Be Filled By O.E.M.', 'Default string', '']:
                identifiers.append(serial)
    except Exception as e:
        print(f"[Hardware ID] Failed to get motherboard serial: {e}")
    
    # 3. MAC Address
    try:
        # Get MAC address as integer
        mac = uuid.getnode()
        # Format as hexadecimal string
        mac_str = ':'.join(['{:02x}'.format((mac >> elements) & 0xff) 
                            for elements in range(0,8*6,8)][::-1])
        identifiers.append(mac_str)
    except Exception as e:
        print(f"[Hardware ID] Failed to get MAC address: {e}")
    
    # 4. CPU ID (Windows)
    try:
        if platform.system() == 'Windows':
            result = subprocess.check_output(
                'wmic cpu get processorid',
                shell=True
            ).decode()
            cpu_id = result.split('\n')[1].strip()
            if cpu_id:
                identifiers.append(cpu_id)
    except Exception as e:
        print(f"[Hardware ID] Failed to get CPU ID: {e}")
    
    # Fallback: Use hostname and machine info if no identifiers found
    if not identifiers:
        print("[Hardware ID] Using fallback method")
        import socket
        identifiers.append(socket.gethostname())
        identifiers.append(platform.machine())
        identifiers.append(platform.processor())
    
    # Combine all identifiers
    combined = '-'.join(identifiers)
    
    # Hash with SHA-256
    hardware_id = hashlib.sha256(combined.encode()).hexdigest()
    
    return hardware_id


def display_hardware_id():
    """
    Display hardware ID for user to send for license generation.
    
    Returns:
        str: The hardware ID
    """
    hw_id = get_hardware_id()
    
    print("\n" + "="*60)
    print("HARDWARE ID")
    print("="*60)
    print(f"\nYour Hardware ID:\n{hw_id}\n")
    print("="*60)
    print("INSTRUCTIONS")
    print("="*60)
    print("1. Copy the Hardware ID above")
    print("2. Send it to the application vendor")
    print("3. You will receive a license.key file")
    print("4. Place license.key in the application directory")
    print("5. Restart the application")
    print("="*60 + "\n")
    
    return hw_id


def verify_hardware_id(expected_hw_id):
    """
    Verify that the current hardware ID matches the expected one.
    
    Args:
        expected_hw_id (str): The expected hardware ID
        
    Returns:
        bool: True if hardware IDs match, False otherwise
    """
    current_hw_id = get_hardware_id()
    return current_hw_id == expected_hw_id


# For testing
if __name__ == '__main__':
    hw_id = get_hardware_id()
    print(f"Hardware ID: {hw_id}")
    print(f"Length: {len(hw_id)} characters")
    display_hardware_id()
