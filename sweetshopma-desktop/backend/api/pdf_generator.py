import io
import os
from reportlab.lib import colors
from reportlab.lib.pagesizes import letter
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer, Image
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import inch
from reportlab.graphics.shapes import Drawing
from reportlab.graphics.charts.piecharts import Pie
from reportlab.graphics.charts.barcharts import VerticalBarChart
from reportlab.graphics import renderPDF
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

# Arabic support
import arabic_reshaper
from bidi.algorithm import get_display

# Register Arial font for Arabic support
FONT_PATH = "C:\\Windows\\Fonts\\arial.ttf"
if os.path.exists(FONT_PATH):
    pdfmetrics.registerFont(TTFont('ArabicFont', FONT_PATH))
    DEFAULT_FONT = 'ArabicFont'
else:
    DEFAULT_FONT = 'Helvetica'

TRANSLATIONS = {
    'en': {
        'report_title': "Attendance Report: {name}",
        'month_label': "Month: {month}",
        'summary_stats': "Summary Statistics",
        'metric': "Metric",
        'value': "Value",
        'days_present': "Days Present",
        'days_absent': "Days Absent",
        'rest_days_earned': "Rest Days Earned",
        'total_reg_hours': "Total Regular Hours",
        'total_ot_hours': "Total Overtime Hours",
        'total_payroll': "Total Payroll",
        'expenses': "Expenses",
        'final_payroll': "Final Payroll",
        'visualization': "Attendance Visualization",
        'present': "Present",
        'absent': "Absent",
        'rest_day': "Rest Day",
        'daily_hours': "Daily Working Hours",
        'legend_hours': "Blue: Regular Hours, Orange: Overtime Hours",
        'detailed_records': "Detailed Attendance Records",
        'date': "Date",
        'status': "Status",
        'in': "In",
        'out': "Out",
        'reg': "Reg",
        'ot': "OT",
        'pay': "Pay",
        'employee': "Employee",
    },
    'ar': {
        'report_title': "تقرير الحضور: {name}",
        'month_label': "الشهر: {month}",
        'summary_stats': "إحصائيات ملخصة",
        'metric': "المقياس",
        'value': "القيمة",
        'days_present': "أيام الحضور",
        'days_absent': "أيام الغياب",
        'rest_days_earned': "أيام الراحة المستحقة",
        'total_reg_hours': "إجمالي الساعات العادية",
        'total_ot_hours': "إجمالي الساعات الإضافية",
        'total_payroll': "إجمالي الراتب",
        'expenses': "المصاريف",
        'final_payroll': "الراتب النهائي",
        'visualization': "تمثيل بياني للحضور",
        'present': "حاضر",
        'absent': "غائب",
        'rest_day': "يوم راحة",
        'daily_hours': "ساعات العمل اليومية",
        'legend_hours': "الأزرق: ساعات عادية، البرتقالي: ساعات إضافية",
        'detailed_records': "سجلات الحضور التفصيلية",
        'date': "التاريخ",
        'status': "الحالة",
        'in': "دخول",
        'out': "خروج",
        'reg': "عادي",
        'ot': "إضافي",
        'pay': "الراتب",
        'employee': "موظف",
    }
}

def _t(text_key, lang='ar', **kwargs):
    """Translate, reshape and reorder text for PDF display."""
    # Normalize language code (e.g., 'ar-SA' -> 'ar')
    lang = (lang or 'ar').split('-')[0].lower()
    
    translations = TRANSLATIONS.get(lang, TRANSLATIONS['en'])
    text = translations.get(text_key, text_key).format(**kwargs)
    
    if lang == 'ar':
        reshaped_text = arabic_reshaper.reshape(text)
        return get_display(reshaped_text)
    return text

def _reverse_if_rtl(row, lang):
    """Reverse row columns if language is RTL."""
    if lang == 'ar':
        return row[::-1]
    return row

def generate_attendance_pdf(buffer, user_data, month, stats, records, lang='ar'):
    """
    Generate a PDF report for attendance with localization.
    """
    # Normalize language code
    lang = (lang or 'ar').split('-')[0].lower()
    
    doc = SimpleDocTemplate(buffer, pagesize=letter)
    elements = []
    
    # Base styles
    styles = getSampleStyleSheet()
    
    # Custom Arabic Styles
    arabic_title_style = ParagraphStyle(
        'ArabicTitle',
        parent=styles['Title'],
        fontName=DEFAULT_FONT,
        alignment=1  # Centered
    )
    
    arabic_normal_style = ParagraphStyle(
        'ArabicNormal',
        parent=styles['Normal'],
        fontName=DEFAULT_FONT,
        alignment=0 if lang == 'en' else 2  # Left for EN, Right for AR
    )
    
    arabic_h2_style = ParagraphStyle(
        'ArabicH2',
        parent=styles['Heading2'],
        fontName=DEFAULT_FONT,
        alignment=0 if lang == 'en' else 2
    )

    arabic_h3_style = ParagraphStyle(
        'ArabicH3',
        parent=styles['Heading3'],
        fontName=DEFAULT_FONT,
        alignment=0 if lang == 'en' else 2
    )
    
    # Title
    name = user_data.get('full_name') or _t('employee', lang)
    elements.append(Paragraph(_t('report_title', lang, name=name), arabic_title_style))
    elements.append(Paragraph(_t('month_label', lang, month=month), arabic_normal_style))
    elements.append(Spacer(1, 12))
    
    # Summary Section
    elements.append(Paragraph(_t('summary_stats', lang), arabic_h2_style))
    
    summary_data = [
        _reverse_if_rtl([_t('metric', lang), _t('value', lang)], lang),
        _reverse_if_rtl([_t('days_present', lang), str(stats.get('days_present', 0))], lang),
        _reverse_if_rtl([_t('days_absent', lang), str(stats.get('days_absent', 0))], lang),
        _reverse_if_rtl([_t('rest_days_earned', lang), str(stats.get('rest_days', 0))], lang),
        _reverse_if_rtl([_t('total_reg_hours', lang), f"{stats.get('total_regular_hours', 0):.2f}"], lang),
        _reverse_if_rtl([_t('total_ot_hours', lang), f"{stats.get('total_overtime_hours', 0):.2f}"], lang),
        _reverse_if_rtl([_t('total_payroll', lang), f"{stats.get('total_payroll', 0):.2f}"], lang),
        _reverse_if_rtl([_t('expenses', lang), f"{stats.get('expenses_total', 0):.2f}"], lang),
        _reverse_if_rtl([_t('final_payroll', lang), f"{stats.get('final_payroll', 0):.2f}"], lang),
    ]
    
    summary_table = Table(summary_data, colWidths=_reverse_if_rtl([200, 100], lang))
    summary_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'LEFT' if lang == 'en' else 'RIGHT'),
        ('FONTNAME', (0, 0), (-1, -1), DEFAULT_FONT),
        ('FONTSIZE', (0, 0), (-1, -1), 10),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
    ]))
    elements.append(summary_table)
    elements.append(Spacer(1, 24))
    
    # Charts Section
    elements.append(Paragraph(_t('visualization', lang), arabic_h2_style))
    
    # Pie Chart: Attendance Status
    drawing_pie = Drawing(400, 200)
    pie = Pie()
    pie.x = 100
    pie.y = 50
    pie.width = 100
    pie.height = 100
    pie.data = [stats.get('days_present', 0), stats.get('days_absent', 0), stats.get('rest_days', 0)]
    pie.labels = [_t('present', lang), _t('absent', lang), _t('rest_day', lang)]
    pie.slices.strokeWidth = 0.5
    pie.slices[0].fillColor = colors.green
    pie.slices[1].fillColor = colors.red
    pie.slices[2].fillColor = colors.blue
    
    # Try setting font for pie labels safely
    try:
        # Some versions use slices.label_fontName, some use slices.fontName
        for i in range(len(pie.data)):
            if hasattr(pie.slices[i], 'label_fontName'):
                pie.slices[i].label_fontName = DEFAULT_FONT
            elif hasattr(pie.slices[i], 'fontName'):
                pie.slices[i].fontName = DEFAULT_FONT
    except:
        pass
        
    drawing_pie.add(pie)
    elements.append(drawing_pie)
    elements.append(Spacer(1, 12))
    
    # Bar Chart: Daily Hours
    if records:
        elements.append(Paragraph(_t('daily_hours', lang), arabic_h3_style))
        
        dates = [r.get('date', '')[8:] for r in records[-15:]]
        reg_hours = [float(r.get('regular_hours', 0)) for r in records[-15:]]
        ot_hours = [float(r.get('overtime_hours', 0)) for r in records[-15:]]
        
        if dates:
            drawing_bar = Drawing(400, 200)
            bc = VerticalBarChart()
            bc.x = 50
            bc.y = 50
            bc.height = 125
            bc.width = 300
            bc.data = [reg_hours, ot_hours]
            bc.strokeColor = colors.black
            bc.valueAxis.valueMin = 0
            bc.valueAxis.valueMax = 12
            bc.valueAxis.valueStep = 2
            
            bc.categoryAxis.labels.boxAnchor = 'ne'
            bc.categoryAxis.labels.dx = 8
            bc.categoryAxis.labels.dy = -2
            bc.categoryAxis.labels.angle = 30
            bc.categoryAxis.categoryNames = dates
            
            # Try setting font for chart axes safely
            try:
                bc.categoryAxis.labels.fontName = DEFAULT_FONT
                bc.valueAxis.labels.fontName = DEFAULT_FONT
            except:
                pass
            
            bc.bars[0].fillColor = colors.blue
            bc.bars[1].fillColor = colors.orange
            drawing_bar.add(bc)
            elements.append(drawing_bar)
            elements.append(Spacer(1, 12))
            elements.append(Paragraph(_t('legend_hours', lang), arabic_normal_style))

    elements.append(Spacer(1, 24))

    # Detailed Records Section
    elements.append(Paragraph(_t('detailed_records', lang), arabic_h2_style))
    
    records_data = [_reverse_if_rtl([
        _t('date', lang), 
        _t('status', lang), 
        _t('in', lang), 
        _t('out', lang), 
        _t('reg', lang), 
        _t('ot', lang), 
        _t('pay', lang)
    ], lang)]
    for r in records:
        status_text = r.get('status', '')
        # Translate status if it's a common one
        status_map = {
            'Present': 'present',
            'Absent': 'absent',
            'Rest Day': 'rest_day'
        }
        status_key = status_map.get(status_text, status_text)
        
        status_display = _t(status_key, lang) if status_key in TRANSLATIONS[lang] else status_text
        
        records_data.append(_reverse_if_rtl([
            r.get('date', ''),
            status_display,
            r.get('check_in_display', '--'),
            r.get('check_out_display', '--'),
            str(r.get('regular_hours', 0)),
            str(r.get('overtime_hours', 0)),
            str(r.get('daily_pay', 0))
        ], lang))
        
    records_table = Table(records_data, colWidths=_reverse_if_rtl([70, 100, 50, 50, 40, 40, 60], lang))
    records_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, -1), DEFAULT_FONT),
        ('FONTSIZE', (0, 0), (-1, -1), 9),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('GRID', (0, 0), (-1, -1), 0.5, colors.black),
    ]))
    
    elements.append(records_table)
    
    doc.build(elements)
