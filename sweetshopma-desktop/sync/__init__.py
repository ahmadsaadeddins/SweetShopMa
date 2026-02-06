"""
Sync Service Module for SweetShopMa Desktop Application

Provides background synchronization with central server.
"""

from .sync_service import SyncService, get_sync_service

__all__ = [
    'SyncService',
    'get_sync_service',
]
