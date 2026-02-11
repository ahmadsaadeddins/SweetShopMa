import io
import os
import sys

# Add backend to path
sys.path.append(os.getcwd())

from api.pdf_generator import generate_attendance_pdf

def test_ar_normalization():
    buffer = io.BytesIO()
    user_data = {'full_name': 'أحمد سعد الدين (Normalized)'}
    month = "2024-02"
    stats = {
        'days_present': 20,
        'days_absent': 2,
        'rest_days': 4,
        'total_regular_hours': 160.0,
        'total_overtime_hours': 10.5,
        'total_payroll': 5000.0,
        'expenses_total': 200.0,
        'final_payroll': 4800.0
    }
    records = []
    
    # Test with ar-SA
    try:
        generate_attendance_pdf(buffer, user_data, month, stats, records, lang='ar-SA')
        print("Arabic PDF (ar-SA) generated successfully")
        
        with open('test_ar_normalized.pdf', 'wb') as f:
            f.write(buffer.getvalue())
        print("Saved to test_ar_normalized.pdf")
        
        return True
    except Exception as e:
        print(f"Error: {e}")
        return False

if __name__ == "__main__":
    test_ar_normalization()
