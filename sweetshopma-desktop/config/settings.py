"""
SweetShopMa Desktop Application Configuration

This file contains all application settings that can be configured
during installation or runtime.
"""

# Application Information
APP_NAME = "SweetShopMa"
APP_VERSION = "1.0.0"
APP_VERSION_DISPLAY = "1.0.0"

# Branch Configuration
# These will be set during installation
BRANCH_ID = None  # Will be set: "branch-1", "branch-2", "branch-3"
BRANCH_NAME = None  # Will be set: "Main Branch", "East Branch", etc.

# Server Configuration
CENTRAL_SERVER_URL = "https://your-server.com/api"  # Change to your server
SYNC_INTERVAL = 300  # 5 minutes in seconds
SYNC_TIMEOUT = 30  # 30 seconds

# Database Configuration
DB_NAME = "sweetshopma.db"
DB_BACKUP_DIR = "backups"
DB_BACKUP_COUNT = 10
DB_BACKUP_INTERVAL = 3600  # 1 hour in seconds

# Security Configuration
LICENSE_FILE = "license.key"
CONFIG_FILE = "app_config.json"
INSTALL_TIME_FILE = "install_config.json"

# Logging Configuration
LOG_DIR = "logs"
LOG_LEVEL = "INFO"  # DEBUG, INFO, WARNING, ERROR, CRITICAL
LOG_MAX_SIZE = 10 * 1024 * 1024  # 10 MB
LOG_BACKUP_COUNT = 5

# Application Settings
WINDOW_WIDTH = 1400
WINDOW_HEIGHT = 800
WINDOW_MIN_WIDTH = 1200
WINDOW_MIN_HEIGHT = 700
WINDOW_BACKGROUND_COLOR = "#f8fafc"

# Django Settings
DJANGO_SECRET_KEY = "django-insecure-change-this-in-production-use-secure-random-key"
DJANGO_DEBUG = False
DJANGO_ALLOWED_HOSTS = ["*"]

# Security Settings
ENABLE_ANTI_DEBUG = True
ENABLE_TAMPER_DETECTION = True
ENABLE_LICENSE_VALIDATION = True

# Sync Settings
ENABLE_AUTO_SYNC = True
SYNC_ON_STARTUP = True
SYNC_ON_SHUTDOWN = True

# Update Settings
ENABLE_AUTO_UPDATE = False
UPDATE_CHECK_INTERVAL = 86400  # 24 hours
UPDATE_SERVER_URL = "https://your-server.com/api/update"

# Performance Settings
CACHE_ENABLED = True
CACHE_TTL = 300  # 5 minutes

# Feature Flags
ENABLE_INVENTORY_MANAGEMENT = True
ENABLE_SALES_TRACKING = True
ENABLE_REPORTS = True
ENABLE_STAFF_MANAGEMENT = True
ENABLE_EXPENSES = True
