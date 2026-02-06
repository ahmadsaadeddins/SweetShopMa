import os
import sys
import django

# Setup Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')
django.setup()

from django.template import engines
from django.conf import settings

print("=" * 60)
print("TEMPLATES Configuration:")
print("=" * 60)
for template_config in settings.TEMPLATES:
    print(f"\nBackend: {template_config['BACKEND']}")
    print(f"DIRS: {template_config['DIRS']}")
    print(f"APP_DIRS: {template_config['APP_DIRS']}")

print("\n" + "=" * 60)
print("Template Engines:")
print("=" * 60)
for engine in engines.all():
    print(f"\nEngine: {engine.engine}")
    if hasattr(engine, 'dirs'):
        print(f"DIRS: {engine.dirs}")

print("\n" + "=" * 60)
print("Installed Apps:")
print("=" * 60)
for app in settings.INSTALLED_APPS:
    print(f"  - {app}")

print("\n" + "=" * 60)
print("Checking for admin templates:")
print("=" * 60)
import django.contrib.admin
admin_path = os.path.dirname(django.contrib.admin.__file__)
admin_templates = os.path.join(admin_path, 'templates', 'admin')
print(f"Admin templates path: {admin_templates}")
print(f"Exists: {os.path.exists(admin_templates)}")
if os.path.exists(admin_templates):
    print(f"change_list.html exists: {os.path.exists(os.path.join(admin_templates, 'change_list.html'))}")
