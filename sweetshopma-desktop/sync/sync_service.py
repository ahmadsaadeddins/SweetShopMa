"""
Sync Service for SweetShopMa Desktop Application

Background service that syncs local data to central server when online.
Runs in a separate thread and handles connectivity detection.
"""

import sys
import threading
import time
import requests
import sqlite3
import json
import os
from datetime import datetime
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent))

from config.settings import (
    BRANCH_ID,
    BRANCH_NAME,
    CENTRAL_SERVER_URL,
    SYNC_INTERVAL,
    SYNC_TIMEOUT,
)

sync_service_instance = None


class SyncService:
    """
    Background service for syncing data to central server.
    
    Features:
    - Automatic connectivity detection
    - Background sync thread
    - Configurable sync interval
    - Sync status tracking
    - Error handling and retries
    """
    
    def __init__(self, branch_id=None, central_server_url=None, local_db_path=None):
        """
        Initialize sync service.
        
        Args:
            branch_id (str): Branch identifier
            central_server_url (str): Central server API URL
            local_db_path (str): Path to local SQLite database
        """
        self.branch_id = branch_id or BRANCH_ID
        self.central_url = central_server_url or CENTRAL_SERVER_URL
        self.local_db = local_db_path or str(Path(__file__).parent.parent / 'sweetshopma.db')
        
        self.running = False
        self.sync_interval = SYNC_INTERVAL  # 5 minutes default
        self.timeout = SYNC_TIMEOUT  # 30 seconds default
        self.last_sync = None
        self.last_successful_sync = None
        self.sync_count = 0
        self.last_error = None
        
        # Thread management
        self.sync_thread = None
        self.stop_event = threading.Event()
        
        # Set global instance
        global sync_service_instance
        sync_service_instance = self
    
    def start(self):
        """Start sync service in background thread"""
        if self.running:
            print("[Sync] Service already running")
            return
        
        self.running = True
        self.stop_event.clear()
        
        # Start sync thread
        self.sync_thread = threading.Thread(
            target=self._sync_loop,
            daemon=True,
            name="SyncService"
        )
        self.sync_thread.start()
        
        print(f"[Sync] Service started for branch {self.branch_id}")
    
    def stop(self):
        """Stop sync service"""
        if not self.running:
            return
        
        self.running = False
        self.stop_event.set()
        
        # Wait for thread to finish (max 5 seconds)
        if self.sync_thread and self.sync_thread.is_alive():
            self.sync_thread.join(timeout=5)
        
        print("[Sync] Service stopped")
    
    def _sync_loop(self):
        """Main sync loop - runs in background thread"""
        while self.running and not self.stop_event.is_set():
            try:
                # Check if online
                if self._is_online():
                    # Perform sync
                    self._sync_data()
                else:
                    print("[Sync] Offline - will retry when connection available")
                
                # Wait for next sync or stop event
                self.stop_event.wait(self.sync_interval)
                
            except Exception as e:
                print(f"[Sync] Error in sync loop: {e}")
                self.last_error = str(e)
                
                # Wait before retrying
                self.stop_event.wait(min(60, self.sync_interval))
    
    def _is_online(self):
        """
        Check if internet connection is available.
        
        Returns:
            bool: True if online, False otherwise
        """
        try:
            response = requests.get(
                f"{self.central_url}/health",
                timeout=3
            )
            return response.status_code == 200
        except requests.exceptions.RequestException:
            return False
        except Exception as e:
            print(f"[Sync] Error checking connectivity: {e}")
            return False
    
    def _sync_data(self):
        """
        Sync local data to central server.
        
        This method:
        1. Gets local changes since last sync
        2. Sends to central server
        3. Updates sync metadata
        """
        try:
            print(f"[Sync] Starting sync at {datetime.now()}")
            
            # Get local changes
            local_changes = self._get_local_changes()
            
            if not local_changes or self._is_empty_changes(local_changes):
                print("[Sync] No new changes to sync")
                self.last_sync = datetime.now()
                return
            
            # Send to central server
            response = self._send_to_server(local_changes)
            
            if response and response.get('status') == 'success':
                # Update sync metadata
                self._update_sync_metadata()
                
                self.last_successful_sync = datetime.now()
                self.last_sync = self.last_successful_sync
                self.sync_count += 1
                self.last_error = None
                
                print(f"[Sync] ✓ Synced successfully (sync #{self.sync_count})")
            else:
                error_msg = response.get('error', 'Unknown error') if response else 'No response'
                print(f"[Sync] ✗ Server error: {error_msg}")
                self.last_error = error_msg
                self.last_sync = datetime.now()
            
        except Exception as e:
            print(f"[Sync] Error during sync: {e}")
            self.last_error = str(e)
            self.last_sync = datetime.now()
    
    def _get_local_changes(self):
        """
        Get all local changes since last sync.
        
        Returns:
            dict: Changes organized by table
        """
        try:
            conn = sqlite3.connect(self.local_db)
            cursor = conn.cursor()
            
            last_sync = self._get_last_sync_timestamp()
            
            changes = {
                'branch_id': self.branch_id,
                'branch_name': self.branch_name,
                'timestamp': datetime.now().isoformat(),
                'sales': self._get_table_changes(cursor, 'api_sale', last_sync),
                'products': self._get_table_changes(cursor, 'api_product', last_sync),
                'expenses': self._get_table_changes(cursor, 'api_expense', last_sync),
            }
            
            conn.close()
            return changes
            
        except Exception as e:
            print(f"[Sync] Error getting local changes: {e}")
            return {}
    
    def _get_table_changes(self, cursor, table, since_timestamp):
        """
        Get changes from a specific table since last sync.
        
        Args:
            cursor: SQLite cursor
            table (str): Table name
            since_timestamp (str): Last sync timestamp
            
        Returns:
            list: List of changed records
        """
        try:
            query = f"""
                SELECT * FROM {table}
                WHERE created_at > ? OR updated_at > ?
                ORDER BY created_at ASC
            """
            cursor.execute(query, (since_timestamp, since_timestamp))
            
            columns = [desc[0] for desc in cursor.description]
            results = cursor.fetchall()
            
            changes = []
            for row in results:
                record = dict(zip(columns, row))
                # Convert datetime objects to strings
                for key, value in record.items():
                    if isinstance(value, datetime):
                        record[key] = value.isoformat()
                changes.append(record)
            
            return changes
            
        except Exception as e:
            print(f"[Sync] Error getting changes from {table}: {e}")
            return []
    
    def _is_empty_changes(self, changes):
        """Check if changes dict is empty"""
        return (
            not changes.get('sales') and
            not changes.get('products') and
            not changes.get('expenses')
        )
    
    def _send_to_server(self, changes):
        """
        Send changes to central server.
        
        Args:
            changes (dict): Changes to send
            
        Returns:
            dict or None: Server response
        """
        try:
            response = requests.post(
                f"{self.central_url}/sync",
                json=changes,
                timeout=self.timeout
            )
            
            if response.status_code == 200:
                return response.json()
            else:
                return {
                    'status': 'error',
                    'error': f"HTTP {response.status_code}: {response.text}"
                }
                
        except requests.exceptions.Timeout:
            return {
                'status': 'error',
                'error': 'Request timeout'
            }
        except requests.exceptions.ConnectionError:
            return {
                'status': 'error',
                'error': 'Connection error'
            }
        except Exception as e:
            return {
                'status': 'error',
                'error': str(e)
            }
    
    def _get_last_sync_timestamp(self):
        """
        Get timestamp of last successful sync.
        
        Returns:
            str: ISO format timestamp
        """
        try:
            conn = sqlite3.connect(self.local_db)
            cursor = conn.cursor()
            
            cursor.execute("""
                SELECT last_sync FROM sync_metadata
                WHERE branch_id = ?
            """, (self.branch_id,))
            
            result = cursor.fetchone()
            conn.close()
            
            return result[0] if result else '1970-01-01 00:00:00'
            
        except Exception as e:
            print(f"[Sync] Error getting last sync timestamp: {e}")
            return '1970-01-01 00:00:00'
    
    def _update_sync_metadata(self):
        """Update sync metadata in local database"""
        try:
            conn = sqlite3.connect(self.local_db)
            cursor = conn.cursor()
            
            now = datetime.now().isoformat()
            
            cursor.execute("""
                INSERT OR REPLACE INTO sync_metadata (branch_id, last_sync, last_successful_sync, sync_count, last_error)
                VALUES (?, ?, ?, ?, ?)
            """, (
                self.branch_id,
                now,
                now,
                self.sync_count + 1,
                None
            ))
            
            conn.commit()
            conn.close()
            
        except Exception as e:
            print(f"[Sync] Error updating sync metadata: {e}")
    
    def sync_now(self):
        """
        Trigger immediate sync (manual sync).
        
        Returns:
            bool: True if sync successful
        """
        try:
            print("[Sync] Manual sync triggered")
            
            if self._is_online():
                self._sync_data()
                return self.last_error is None
            else:
                print("[Sync] Cannot sync - offline")
                return False
                
        except Exception as e:
            print(f"[Sync] Error in manual sync: {e}")
            return False
    
    def get_sync_status(self):
        """
        Get current sync status for UI display.
        
        Returns:
            dict: Sync status information
        """
        return {
            'branch_id': self.branch_id,
            'branch_name': self.branch_name,
            'is_online': self._is_online(),
            'is_syncing': self.running,
            'last_sync': self.last_sync.isoformat() if self.last_sync else None,
            'last_successful_sync': self.last_successful_sync.isoformat() if self.last_successful_sync else None,
            'sync_count': self.sync_count,
            'last_error': self.last_error,
            'sync_interval': self.sync_interval,
        }
    
    def set_sync_interval(self, seconds):
        """
        Update sync interval.
        
        Args:
            seconds (int): New sync interval in seconds
        """
        self.sync_interval = max(60, seconds)  # Minimum 1 minute
        print(f"[Sync] Sync interval updated to {self.sync_interval} seconds")


# Convenience function to get global instance
def get_sync_service():
    """Get the global sync service instance"""
    return sync_service_instance


# For testing
if __name__ == '__main__':
    print("Sync Service Test")
    print("="*60)
    
    # Create sync service
    sync_service = SyncService(
        branch_id='test-branch-1',
        central_server_url='https://your-server.com/api',
        local_db_path='sweetshopma.db'
    )
    
    # Test connectivity
    print("\n1. Testing connectivity...")
    is_online = sync_service._is_online()
    print(f"   Online: {is_online}")
    
    # Test sync status
    print("\n2. Getting sync status...")
    status = sync_service.get_sync_status()
    print(f"   Branch ID: {status['branch_id']}")
    print(f"   Online: {status['is_online']}")
    print(f"   Sync Count: {status['sync_count']}")
    
    print("\n" + "="*60)
    print("Sync Service test complete")
