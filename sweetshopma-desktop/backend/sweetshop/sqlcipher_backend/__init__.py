"""
SQLCipher database backend for Django.

This package provides a custom database backend that uses sqlcipher3
for encrypted SQLite databases.
"""

from .base import DatabaseWrapper

__all__ = ['DatabaseWrapper']
