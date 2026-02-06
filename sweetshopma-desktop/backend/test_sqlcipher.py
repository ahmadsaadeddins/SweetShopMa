"""
Diagnostic script to test sqlcipher3 installation and configuration.

Run this script to verify that sqlcipher3 is properly installed and working.
"""

import sys
import os
import io

# Fix Windows console encoding issue
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

print("=" * 70)
print("SQLCIPHER3 DIAGNOSTIC TEST")
print("=" * 70)

# Test 1: Check if sqlcipher3 is installed
print("\n[TEST 1] Checking if sqlcipher3 is installed...")
try:
    import sqlcipher3
    print("  ✓ sqlcipher3 is installed")
    print(f"  Module path: {sqlcipher3.__file__}")
    print(f"  SQLite version: {sqlcipher3.sqlite_version}")
except ImportError as e:
    print(f"  ✗ sqlcipher3 is NOT installed")
    print(f"  Error: {e}")
    print("\n  To install: pip install sqlcipher3")
    sys.exit(1)
except Exception as e:
    print(f"  ✗ Error importing sqlcipher3: {e}")
    sys.exit(1)

# Test 2: Check if sqlcipher3 patches sqlite3
print("\n[TEST 2] Checking if sqlcipher3 patches sqlite3...")
try:
    # Import sqlcipher3 first (it should patch sqlite3)
    import sqlcipher3
    
    # Now import sqlite3
    import sqlite3
    
    # Check if they're the same module
    if sqlite3.__file__ == sqlcipher3.__file__:
        print("  ✓ sqlite3 is patched with sqlcipher3")
        print(f"  Both modules point to: {sqlite3.__file__}")
    else:
        print("  ⚠ sqlite3 is NOT patched with sqlcipher3")
        print(f"  sqlcipher3: {sqlcipher3.__file__}")
        print(f"  sqlite3:    {sqlite3.__file__}")
        print("  This may cause issues with Django")
except Exception as e:
    print(f"  ✗ Error checking patch: {e}")

# Test 3: Test creating an encrypted database
print("\n[TEST 3] Testing encrypted database creation...")
try:
    import sqlcipher3.dbapi2 as sqlite
    
    # Create a test database
    test_db_path = os.path.join(os.path.dirname(__file__), 'test_encrypted.db')
    test_key = 'test-encryption-key-123'
    
    # Delete test database if it exists
    if os.path.exists(test_db_path):
        os.remove(test_db_path)
    
    # Create encrypted database
    conn = sqlite.connect(test_db_path)
    cursor = conn.cursor()
    
    # Set encryption key
    cursor.execute(f'PRAGMA key = "{test_key}"')
    print("  ✓ Encryption key set via PRAGMA")
    
    # Create a test table
    cursor.execute('CREATE TABLE test_table (id INTEGER PRIMARY KEY, name TEXT)')
    print("  ✓ Created test table")
    
    # Insert test data
    cursor.execute('INSERT INTO test_table (name) VALUES (?)', ('Test Data',))
    conn.commit()
    print("  ✓ Inserted test data")
    
    # Close connection
    conn.close()
    print("  ✓ Database closed successfully")
    
    # Test 4: Try to open with correct key
    print("\n[TEST 4] Testing database access with correct key...")
    conn = sqlite.connect(test_db_path)
    cursor = conn.cursor()
    cursor.execute(f'PRAGMA key = "{test_key}"')
    
    cursor.execute('SELECT * FROM test_table')
    rows = cursor.fetchall()
    if len(rows) == 1 and rows[0][1] == 'Test Data':
        print("  ✓ Successfully read data with correct key")
    else:
        print("  ✗ Failed to read data correctly")
    conn.close()
    
    # Test 5: Try to open with wrong key
    print("\n[TEST 5] Testing database access with wrong key...")
    conn = sqlite.connect(test_db_path)
    cursor = conn.cursor()
    cursor.execute(f'PRAGMA key = "wrong-key"')
    
    try:
        cursor.execute('SELECT * FROM test_table')
        rows = cursor.fetchall()
        print("  ⚠ WARNING: Database opened with wrong key!")
        print("  This means encryption is NOT working properly")
    except Exception as e:
        print(f"  ✓ Correctly rejected wrong key: {type(e).__name__}")
    finally:
        conn.close()
    
    # Clean up
    if os.path.exists(test_db_path):
        os.remove(test_db_path)
        print("\n  ✓ Test database cleaned up")
    
except Exception as e:
    print(f"  ✗ Error during database test: {e}")
    import traceback
    traceback.print_exc()

# Test 6: Check Django settings
print("\n[TEST 6] Checking Django configuration...")
try:
    # Add parent directory to path
    sys.path.insert(0, os.path.dirname(os.path.dirname(__file__)))
    
    os.environ.setdefault('DJANGO_SECRET_KEY', 'test-secret-key')
    os.environ.setdefault('DJANGO_DEBUG', 'False')
    
    from django.conf import settings
    
    # Check database backend
    db_backend = settings.DATABASES['default']['ENGINE']
    print(f"  Database backend: {db_backend}")
    
    if 'sqlcipher' in db_backend.lower():
        print("  ✓ Django configured to use SQLCipher backend")
    else:
        print("  ⚠ Django using standard sqlite3 backend")
        print("  Encryption may not work!")
    
    # Check if key is configured
    db_options = settings.DATABASES['default'].get('OPTIONS', {})
    if 'key' in db_options:
        print(f"  ✓ Encryption key configured (length: {len(db_options['key'])})")
    else:
        print("  ⚠ No encryption key found in database OPTIONS")
    
except Exception as e:
    print(f"  ⚠ Could not check Django settings: {e}")

print("\n" + "=" * 70)
print("DIAGNOSTIC TEST COMPLETE")
print("=" * 70)
print("\nIf all tests passed with ✓, sqlcipher3 is working correctly.")
print("If you see ✗ or ⚠, there may be issues that need to be addressed.")
print("\nNext steps:")
print("1. If sqlcipher3 is not installed: pip install sqlcipher3")
print("2. If encryption is not working: Check Django backend configuration")
print("3. Run Django migrations: python manage.py migrate")
print("=" * 70)
