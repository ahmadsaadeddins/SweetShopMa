"""
Custom Django database backend for SQLCipher using sqlcipher3 package.

This backend wraps Django's sqlite3 backend and adds SQLCipher encryption support.
"""

import sys
from django.db.backends.sqlite3.base import DatabaseWrapper as SQLiteDatabaseWrapper
from django.db.backends.sqlite3.base import DatabaseFeatures as SQLiteDatabaseFeatures
from django.db.backends.sqlite3._functions import register as register_functions


class DatabaseFeatures(SQLiteDatabaseFeatures):
    """
    Custom database features for sqlcipher3.

    Overrides methods that don't work with sqlcipher3.
    """

    @property
    def max_query_params(self):
        """
        Return the maximum number of parameters for a query.

        sqlcipher3 doesn't have getlimit(), so we return a safe default.
        SQLite's default is typically 999 or 32766 depending on version.
        """
        return 999  # Safe default for SQLite


class SQLCipherCursorWrapper:
    """
    Custom cursor wrapper for sqlcipher3 that handles Django's parameter substitution.

    This is needed because Django's SQLiteCursorWrapper does type checking that
    rejects sqlcipher3 cursors.

    Django uses %(name)s style parameters (pyformat), but sqlcipher3 uses ? style (qmark).
    We need to convert between these formats.
    """

    def __init__(self, cursor):
        self.cursor = cursor

    def _convert_params(self, query, params):
        """
        Convert Django's format/pyformat parameters to qmark style.

        Django uses %s or %(name)s style, but sqlcipher3 uses ? style.
        Also converts Decimal to float since sqlcipher3 doesn't support Decimal binding.
        """
        from decimal import Decimal
        
        if params is None:
            return query, None

        # Helper function to convert Decimal to float
        def convert_value(value):
            if isinstance(value, Decimal):
                return float(value)
            return value

        # Check if query uses %s style placeholders (format)
        if '%s' in query:
            # Count the number of %s placeholders
            count = query.count('%s')
            # If params is a list/tuple, use it directly
            if isinstance(params, (list, tuple)):
                # Convert Decimal values to float
                params = tuple(convert_value(p) for p in params)
                # Replace all %s with ?
                query = query.replace('%s', '?')
                return query, params
            # If params is a dict, we need to extract values in order
            # This shouldn't happen with %s style, but handle it anyway
            elif isinstance(params, dict):
                # This is unusual - %s with dict params
                # Just replace %s with ? and use dict values
                query = query.replace('%s', '?')
                params = tuple(convert_value(v) for v in params.values())
                return query, params

        # Check if query uses %(name)s style placeholders (pyformat)
        elif '%(' in query:
            import re
            # Replace %(name)s with ? and build params tuple
            param_list = []
            # Find all %(name)s placeholders in order
            placeholders = re.findall(r'%\((\w+)\)s', query)
            for key in placeholders:
                param_list.append(convert_value(params[key]))
            # Replace all %(name)s with ?
            query = re.sub(r'%\(\w+\)s', '?', query)
            return query, tuple(param_list)

        return query, params

    def execute(self, query, params=None):
        """Execute a query with parameters."""
        query, params = self._convert_params(query, params)
        # Debug logging
        if '%' in query:
            print(f"[SQLCipherCursor] Query contains %: {query[:200]}")
            print(f"[SQLCipherCursor] Params: {params}")
        if params is None:
            return self.cursor.execute(query)
        else:
            return self.cursor.execute(query, params)

    def executemany(self, query, param_list):
        """Execute a query with multiple parameter sets."""
        # Convert each set of parameters
        converted_list = []
        query_converted = False
        for params in param_list:
            q, p = self._convert_params(query, params)
            if not query_converted:
                query = q
                query_converted = True
            converted_list.append(p if p is not None else ())
        return self.cursor.executemany(query, converted_list)

    def __getattr__(self, name):
        """Delegate all other attributes to the underlying cursor."""
        return getattr(self.cursor, name)

    def __iter__(self):
        """Make the cursor iterable."""
        return iter(self.cursor)


class DatabaseWrapper(SQLiteDatabaseWrapper):
    """
    Custom database wrapper that uses sqlcipher3 instead of sqlite3.

    This explicitly uses sqlcipher3.dbapi2 for encrypted database connections.
    """

    # Use custom features class
    features_class = DatabaseFeatures

    # Explicitly use sqlcipher3's dbapi2 module
    Database = None  # Will be set in __init__

    def __init__(self, *args, **kwargs):
        # Import sqlcipher3.dbapi2 to use as the database module
        try:
            import sqlcipher3.dbapi2 as Database
            DatabaseWrapper.Database = Database
            print(f"[SQLCipherBackend] [OK] Using sqlcipher3.dbapi2 (SQLite {Database.sqlite_version})")
        except ImportError as e:
            print(f"[SQLCipherBackend] [ERROR] Failed to import sqlcipher3: {e}")
            print("[SQLCipherBackend] Database will NOT be encrypted!")
            # Fall back to standard sqlite3
            import sqlite3 as Database
            DatabaseWrapper.Database = Database

        super().__init__(*args, **kwargs)

    def get_new_connection(self, conn_params):
        """
        Override to create connection using sqlcipher3 and set encryption key.
        """
        # Remove 'key' from conn_params as it's not a valid connect() argument
        # The key will be set via PRAGMA after connection
        conn_params = {k: v for k, v in conn_params.items() if k != 'key'}

        # Use sqlcipher3 to create the connection
        conn = self.Database.connect(**conn_params)

        # Set encryption key if provided in OPTIONS
        encryption_key = self.settings_dict.get('OPTIONS', {}).get('key')
        if encryption_key:
            try:
                # Set the encryption key using PRAGMA
                cursor = conn.cursor()
                cursor.execute(f'PRAGMA key = "{encryption_key}"')
                cursor.close()
                print("[SQLCipherBackend] [OK] Encryption key set successfully")
            except Exception as e:
                print(f"[SQLCipherBackend] [ERROR] Failed to set encryption key: {e}")
                print("[SQLCipherBackend] Database may not be properly encrypted!")

        # Register Django's custom SQL functions
        try:
            register_functions(conn)
            print("[SQLCipherBackend] [OK] Django SQL functions registered")
        except Exception as e:
            print(f"[SQLCipherBackend] [WARNING] Failed to register SQL functions: {e}")

        return conn

    def create_cursor(self, name=None):
        """
        Override to create cursor that works with sqlcipher3 connection.

        We wrap the cursor with our custom wrapper that handles parameter substitution.
        """
        # Create a raw cursor from the connection
        raw_cursor = self.connection.cursor()
        # Wrap it with our custom cursor wrapper
        return SQLCipherCursorWrapper(raw_cursor)
