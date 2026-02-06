"""
Anti-Debugging Module

Detects and prevents debugging attempts to protect the application
from reverse engineering and tampering.
"""

import sys
import ctypes
import os


def detect_debugger():
    """
    Detect if a debugger is attached to the process.
    
    Checks for:
    1. Windows debugger (IsDebuggerPresent)
    2. Remote debugger (CheckRemoteDebuggerPresent)
    3. Python tracing (sys.gettrace)
    
    Exits immediately if debugger is detected.
    """
    debugger_detected = False
    
    # Check 1: Windows debugger
    if sys.platform == 'win32':
        try:
            kernel32 = ctypes.windll.kernel32
            
            # Check for local debugger
            is_debugger_present = kernel32.IsDebuggerPresent()
            if is_debugger_present:
                print("[Security] Debugger detected!")
                debugger_detected = True
            
            # Check for remote debugger
            check_remote_debugger = kernel32.CheckRemoteDebuggerPresent
            is_remote_debugger = ctypes.c_long(0)
            check_remote_debugger(None, ctypes.byref(is_remote_debugger))
            
            if is_remote_debugger.value:
                print("[Security] Remote debugger detected!")
                debugger_detected = True
                
        except Exception as e:
            print(f"[Security] Error checking for debugger: {e}")
    
    # Check 2: Python tracing (used by debuggers)
    if sys.gettrace():
        print("[Security] Trace function detected (possible debugger)!")
        debugger_detected = True
    
    # Check 3: Check for common debugger environment variables
    debug_env_vars = ['PYCHARM_HOSTED', 'PYDEV_HOSTED', 'DEBUG']
    for var in debug_env_vars:
        if os.environ.get(var):
            print(f"[Security] Debug environment detected: {var}")
            # Don't exit for this, just warn
            # debugger_detected = True
    
    # Exit if debugger detected
    if debugger_detected:
        print("\n" + "="*60)
        print("SECURITY ALERT")
        print("="*60)
        print("Debugger detected. Application cannot run under debugger.")
        print("This is to protect the application from reverse engineering.")
        print("="*60 + "\n")
        sys.exit(1)


def check_integrity():
    """
    Check application integrity.
    
    This is a placeholder for more advanced integrity checks.
    In a production application, you could:
    - Verify checksums of critical files
    - Check for modified code
    - Validate the executable signature
    
    Returns:
        bool: True if integrity check passes
    """
    # Placeholder: Always return True for now
    # In production, implement actual integrity checks
    return True


def prevent_patching():
    """
    Prevent memory patching and code modification.
    
    This is a placeholder for anti-patching measures.
    In a production application, you could:
    - Use code obfuscation (PyArmor)
    - Implement checksums of critical functions
    - Use anti-tampering techniques
    
    Returns:
        bool: True if anti-patching is enabled
    """
    # Placeholder: Always return True for now
    # PyArmor will handle most of this
    return True


def enable_protections():
    """
    Enable all security protections.
    
    This function should be called at application startup
    to enable all anti-debugging and anti-tampering measures.
    """
    # Check for debugger
    detect_debugger()
    
    # Check integrity
    if not check_integrity():
        print("[Security] Integrity check failed!")
        sys.exit(1)
    
    # Enable anti-patching
    if not prevent_patching():
        print("[Security] Anti-patching failed!")
        sys.exit(1)
    
    print("[Security] All protections enabled.")


# For testing
if __name__ == '__main__':
    print("Anti-Debug Test")
    print("="*60)
    print("\nAttempting to detect debugger...")
    
    try:
        detect_debugger()
        print("✓ No debugger detected")
    except SystemExit:
        print("✗ Debugger detected - application would exit")
    
    print("\n" + "="*60)
