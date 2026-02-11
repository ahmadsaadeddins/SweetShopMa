import os
import django
import sys

# Setup Django environment
sys.path.append(os.getcwd())
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'backend.settings')
django.setup()

from api.models import AttendanceRecord
from decimal import Decimal

def fix_records():
    print("Checking for incomplete attendance records...")
    
    # records with 0 pay but Present
    zero_pay = AttendanceRecord.objects.filter(status='Present', daily_pay=Decimal('0.00'))
    print(f"Found {zero_pay.count()} records with 0.00 pay.")
    
    # records with 8 hours but no check-in
    no_time = AttendanceRecord.objects.filter(status='Present', regular_hours=Decimal('8.00'), check_in_time__isnull=True)
    print(f"Found {no_time.count()} records with 8 hours but no timestamps.")
    
    # Union to avoid double saving if possible, but simple iteration is fine
    # actually, save() handles both now.
    
    count = 0
    # Process all Present records just to be safe? 
    # Or just the zero pay ones (which covers most) and the no-time ones.
    
    records_to_fix = (zero_pay | no_time).distinct()
    
    print(f"Total unique records to process: {records_to_fix.count()}")
    
    for record in records_to_fix:
        print(f"Fixing {record.date} for {record.user_name}...")
        record.save() # This triggers the new logic
        count += 1
        
    print(f"Successfully processed {count} records.")

if __name__ == '__main__':
    try:
        fix_records()
    except Exception as e:
        print(f"Error: {e}")
