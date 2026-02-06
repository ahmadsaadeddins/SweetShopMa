"""
Django settings for SweetShopMa Desktop Application.

This configuration uses SQLCipher for encrypted database storage
and is optimized for desktop application use.
"""

import os
import sys
from pathlib import Path

# Build paths
BASE_DIR = Path(__file__).resolve().parent.parent
PROJECT_ROOT = BASE_DIR.parent

# Add parent directory to path for imports
sys.path.insert(0, str(PROJECT_ROOT))

# Load environment variables from .env file if it exists
# This allows configuration without setting system environment variables
env_file = PROJECT_ROOT / '.env'
if env_file.exists():
    print(f"[Django] Loading environment variables from: {env_file}")
    with open(env_file, 'r') as f:
        for line in f:
            line = line.strip()
            if line and not line.startswith('#') and '=' in line:
                key, value = line.split('=', 1)
                os.environ.setdefault(key.strip(), value.strip())

# SECURITY WARNING: keep the secret key used in production secret!
# CRITICAL: DJANGO_SECRET_KEY MUST be set as environment variable
# The application will NOT start without it for security reasons.
SECRET_KEY = os.environ.get('DJANGO_SECRET_KEY')

if not SECRET_KEY:
    raise RuntimeError(
        "CRITICAL: DJANGO_SECRET_KEY environment variable is not set!\n"
        "For security reasons, this application requires a secret key.\n"
        "Generate one using: python generate_secret_key.py\n"
        "Then set it: export DJANGO_SECRET_KEY='your-generated-key'  (Linux/Mac)\n"
        "             set DJANGO_SECRET_KEY=your-generated-key       (Windows)\n"
        "Or create a .env file in the sweetshopma-desktop directory."
    )

# SECURITY WARNING: don't run with debug turned on in production!
DEBUG = os.environ.get('DJANGO_DEBUG', 'True') == 'True'

ALLOWED_HOSTS = ['*']

# Application definition
INSTALLED_APPS = [
    'django.contrib.staticfiles',  # Required for static file serving
    'django.contrib.admin',
    'django.contrib.auth',
    'django.contrib.contenttypes',
    'django.contrib.sessions',
    'django.contrib.messages',
    'rest_framework',
    'rest_framework.authtoken',  # Token authentication for PyWebView
    'corsheaders',  # Add CORS support
    'api',
]

MIDDLEWARE = [
    'django.middleware.security.SecurityMiddleware',
    'corsheaders.middleware.CorsMiddleware',  # CORS middleware (must be before CommonMiddleware)
    'django.contrib.sessions.middleware.SessionMiddleware',
    'django.middleware.common.CommonMiddleware',
    'django.middleware.csrf.CsrfViewMiddleware',
    'django.contrib.auth.middleware.AuthenticationMiddleware',
    'django.contrib.messages.middleware.MessageMiddleware',
    'django.middleware.clickjacking.XFrameOptionsMiddleware',
]

ROOT_URLCONF = 'sweetshop.urls'

TEMPLATES = [
    {
        'BACKEND': 'django.template.backends.django.DjangoTemplates',
        'DIRS': [],
        'APP_DIRS': True,
        'OPTIONS': {
            'context_processors': [
                'django.template.context_processors.debug',
                'django.template.context_processors.request',
                'django.contrib.auth.context_processors.auth',
                'django.contrib.messages.context_processors.messages',
            ],
        },
    },
]

WSGI_APPLICATION = 'sweetshop.wsgi.application'

# Database - SQLCipher with encrypted key
# The encryption key is derived from hardware ID + license + install time

# DIAGNOSTIC: Check if sqlcipher3 is available
print("[Django] ==================================================")
print("[Django] DIAGNOSTIC: Checking SQLCipher availability...")
try:
    import sqlcipher3
    print("[Django] [OK] sqlcipher3 module found")
    print(f"[Django]   sqlcipher3 version: {sqlcipher3.sqlite_version}")
except ImportError as e:
    print(f"[Django] [ERROR] sqlcipher3 NOT found: {e}")
    print(f"[Django]   Database will use standard sqlite3 (NO ENCRYPTION)")
except Exception as e:
    print(f"[Django] [ERROR] Error checking sqlcipher3: {e}")

# DIAGNOSTIC: Check if standard sqlite3 is being used
try:
    import sqlite3
    print(f"[Django]   sqlite3 module path: {sqlite3.__file__}")
    print(f"[Django]   sqlite3 version: {sqlite3.sqlite_version}")
except Exception as e:
    print(f"[Django]   Error checking sqlite3: {e}")

# Load encryption key
try:
    from security.key_manager import KeyManager
    encryption_key = KeyManager.get_encryption_key()
    print(f"[Django] [OK] Encryption key loaded (length: {len(encryption_key)} chars)")
except Exception as e:
    print(f"[Django] [ERROR] Warning: Could not load encryption key: {e}")
    print(f"[Django]   Using fallback key (database will not be properly encrypted)")
    encryption_key = "fallback-key-do-not-use-in-production"

DATABASES = {
    'default': {
        'ENGINE': 'sweetshop.sqlcipher_backend',  # Use custom SQLCipher backend
        'NAME': os.path.join(PROJECT_ROOT, 'sweetshopma.db'),
        'OPTIONS': {
            'key': encryption_key,
        },
        'ATOMIC_REQUESTS': True,  # Enable transaction management
    }
}
print(f"[Django] Database configured: {DATABASES['default']['NAME']}")
print(f"[Django] Backend: {DATABASES['default']['ENGINE']}")
print("[Django] ==================================================")

# Password validation
AUTH_PASSWORD_VALIDATORS = [
    {'NAME': 'django.contrib.auth.password_validation.UserAttributeSimilarityValidator'},
    {'NAME': 'django.contrib.auth.password_validation.MinimumLengthValidator'},
    {'NAME': 'django.contrib.auth.password_validation.CommonPasswordValidator'},
    {'NAME': 'django.contrib.auth.password_validation.NumericPasswordValidator'},
]

# Internationalization
LANGUAGE_CODE = 'en-us'
TIME_ZONE = 'UTC'
USE_I18N = True
USE_TZ = True

# Static files (CSS, JavaScript, Images)
STATIC_URL = 'static/'
STATIC_ROOT = os.path.join(PROJECT_ROOT, 'frontend', 'static')

# Additional static files directories for development
STATICFILES_DIRS = [
    os.path.join(BASE_DIR, 'static'),
]

# Default primary key field type
DEFAULT_AUTO_FIELD = 'django.db.models.BigAutoField'

# REST Framework Configuration
REST_FRAMEWORK = {
    'DEFAULT_PAGINATION_CLASS': 'rest_framework.pagination.PageNumberPagination',
    'PAGE_SIZE': 100,
    'DEFAULT_RENDERER_CLASSES': [
        'rest_framework.renderers.JSONRenderer',
    ],
    'DEFAULT_PARSER_CLASSES': [
        'rest_framework.parsers.JSONParser',
    ],
    'DEFAULT_AUTHENTICATION_CLASSES': [
        'rest_framework.authentication.TokenAuthentication',
        'rest_framework.authentication.SessionAuthentication',
    ],
}

# CORS Configuration - Restrict to localhost for development, specific origins for production
# For PyWebView desktop app, these are the expected origins
# SECURITY: Changed from CORS_ALLOW_ALL_ORIGINS = True to restrict access
CORS_ALLOW_ALL_ORIGINS = False  # SECURITY: Disabled - was allowing any origin
CORS_ALLOW_CREDENTIALS = True
CORS_ALLOWED_ORIGINS = [
    "http://localhost:3000",
    "http://127.0.0.1:3000",
    "http://localhost:5173",  # Vite default
    "http://127.0.0.1:5173",
    "http://localhost:8000",  # Django admin/dev server
    "http://127.0.0.1:8000",
]

# For PyWebView which may use file:// or empty origin
CORS_ALLOW_ALL_ORIGINS = os.environ.get('DJANGO_DEBUG', 'False') == 'True'  # Only allow all in debug mode

CSRF_TRUSTED_ORIGINS = [
    "http://127.0.0.1:8000",
    "http://localhost:8000",
]

# Logging Configuration
LOGGING = {
    'version': 1,
    'disable_existing_loggers': False,
    'formatters': {
        'verbose': {
            'format': '{levelname} {asctime} {module} {message}',
            'style': '{',
        },
    },
    'handlers': {
        'file': {
            'level': 'INFO',
            'class': 'logging.FileHandler',
            'filename': os.path.join(PROJECT_ROOT, 'logs', 'django.log'),
            'formatter': 'verbose',
        },
        'console': {
            'level': 'INFO',
            'class': 'logging.StreamHandler',
            'formatter': 'verbose',
        },
    },
    'root': {
        'handlers': ['console', 'file'],
        'level': 'INFO',
    },
    'loggers': {
        'django': {
            'handlers': ['console', 'file'],
            'level': 'INFO',
            'propagate': False,
        },
        'api': {
            'handlers': ['console', 'file'],
            'level': 'INFO',
            'propagate': False,
        },
    },
}

# Create logs directory if it doesn't exist
os.makedirs(os.path.join(PROJECT_ROOT, 'logs'), exist_ok=True)

# Application Configuration
from config.settings import (
    BRANCH_ID,
    BRANCH_NAME,
    APP_VERSION,
    ENABLE_INVENTORY_MANAGEMENT,
    ENABLE_SALES_TRACKING,
    ENABLE_REPORTS,
)

# Custom settings
SWEETSHOP = {
    'BRANCH_ID': BRANCH_ID,
    'BRANCH_NAME': BRANCH_NAME,
    'VERSION': APP_VERSION,
    'FEATURES': {
        'INVENTORY': ENABLE_INVENTORY_MANAGEMENT,
        'SALES': ENABLE_SALES_TRACKING,
        'REPORTS': ENABLE_REPORTS,
    }
}
