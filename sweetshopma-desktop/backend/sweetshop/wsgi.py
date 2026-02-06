"""
WSGI config for SweetShopMa Desktop Application.
"""

import os

from django.core.wsgi import get_wsgi_application

os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')

application = get_wsgi_application()
