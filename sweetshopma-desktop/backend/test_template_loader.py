import os
import sys
import django

# Setup Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')
django.setup()

from django.template import loader
from django.contrib.admin import site

print("=" * 60)
print("Testing Template Loader")
print("=" * 60)

# Try to find the admin change_list template
template_names = [
    'admin/api/product/change_list.html',
    'admin/api/change_list.html',
    'admin/change_list.html',
]

for template_name in template_names:
    print(f"\nLooking for: {template_name}")
    try:
        template = loader.get_template(template_name)
        print(f"  [OK] FOUND: {template.origin}")
    except Exception as e:
        print(f"  [ERROR] NOT FOUND: {e}")

# Try to find the base admin change_list template
print("\n" + "=" * 60)
print("Testing base admin template")
print("=" * 60)
try:
    template = loader.select_template(['admin/change_list.html'])
    print(f"[OK] Base admin template found: {template.origin}")
except Exception as e:
    print(f"[ERROR] Base admin template NOT FOUND: {e}")

# Check template loaders
print("\n" + "=" * 60)
print("Template Loaders")
print("=" * 60)
from django.template.engine import Engine
engine = Engine.get_default()
for loader in engine.template_loaders:
    print(f"  Loader: {loader}")

# Check app directories
print("\n" + "=" * 60)
print("App Directories")
print("=" * 60)
from django.template.loaders import app_directories
for app_dir in app_directories.get_app_template_dirs('templates'):
    print(f"  {app_dir}")
    if 'admin' in str(app_dir):
        admin_template_dir = os.path.join(app_dir, 'admin')
        if os.path.exists(admin_template_dir):
            print(f"    -> admin/ exists")
            change_list = os.path.join(admin_template_dir, 'change_list.html')
            if os.path.exists(change_list):
                print(f"    -> change_list.html exists")
