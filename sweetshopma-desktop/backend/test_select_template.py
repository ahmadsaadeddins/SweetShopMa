import os
import sys
import django

# Setup Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'sweetshop.settings')
django.setup()

from django.template import loader

print("=" * 60)
print("Testing select_template (as Django admin uses)")
print("=" * 60)

# This is what Django admin does - it tries to select from multiple templates
template_names = [
    'admin/api/product/change_list.html',
    'admin/api/change_list.html',
    'admin/change_list.html',
]

print(f"\nTrying to select from: {template_names}")
try:
    template = loader.select_template(template_names)
    print(f"[OK] Template found: {template.origin}")
except Exception as e:
    print(f"[ERROR] No template found: {e}")
    print(f"Exception type: {type(e).__name__}")

# Now let's check what happens if we only try the base template
print("\n" + "=" * 60)
print("Testing with just base template")
print("=" * 60)
try:
    template = loader.select_template(['admin/change_list.html'])
    print(f"[OK] Template found: {template.origin}")
except Exception as e:
    print(f"[ERROR] No template found: {e}")
