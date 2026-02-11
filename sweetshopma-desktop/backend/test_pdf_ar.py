import io
import os
import sys

# Add backend to path
sys.path.append(os.getcwd())

from api.pdf_generator import generate_attendance_pdf

def test_ar_pdf():
    buffer = io.BytesIO()
    user_data = {'full_name': 'أحمد سعد الدين'}
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
    records = [
        {'date': '2024-02-01', 'status': 'Present', 'check_in_display': '08:00', 'check_out_display': '17:00', 'regular_hours': 8, 'overtime_hours': 1, 'daily_pay': 200},
        {'date': '2024-02-02', 'status': 'Absent', 'check_in_display': '--', 'check_out_display': '--', 'regular_hours': 0, 'overtime_hours': 0, 'daily_pay': 0},
    ]
    
    try:
        generate_attendance_pdf(buffer, user_data, month, stats, records, lang='ar')
        print("Arabic PDF generated successfully to buffer")
        
        # Save to disk to manually check if possible or just verify it didn't crash
        with open('test_ar_attendance.pdf', 'wb') as f:
            f.write(buffer.getvalue())
        print("Saved to test_ar_attendance.pdf")
        
        return True
    except Exception as e:
        print(f"Error generating Arabic PDF: {e}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    test_ar_pdf()
