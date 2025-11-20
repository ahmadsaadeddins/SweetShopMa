'use client'

import React, { createContext, useContext, useState, useEffect } from 'react'

type Language = 'en' | 'ar'

interface LocalizationContextType {
  language: Language
  setLanguage: (lang: Language) => void
  isRTL: boolean
  t: (key: string) => string
}

const translations: Record<Language, Record<string, string>> = {
  en: {
    // Authentication
    Login: 'Login',
    Username: 'Username',
    Password: 'Password',
    Logout: 'Logout',
    LoggedInAs: 'Logged in as',
    
    // Shop
    Shop: 'Shop',
    Cart: 'Cart',
    Checkout: 'Checkout',
    Total: 'Total',
    Add: 'Add',
    BarcodeSearch: 'Barcode / Search',
    ScanBarcode: 'Scan barcode or search...',
    Products: 'Products',
    Quantity: 'Quantity',
    Stock: 'Stock',
    CartEmpty: 'Cart is empty',
    ConfirmCheckout: 'Checkout',
    OrderPlaced: 'Order placed successfully!',
    PrintReceipt: 'Print Receipt',
    
    // Admin
    AdminPanel: 'Admin Panel',
    ReportsInsights: 'Reports & Insights',
    Users: 'Users',
    UserManagement: 'User Management',
    TotalSales: 'Total Sales',
    TotalOrders: 'Total Orders',
    AverageOrderValue: 'Avg Order Value',
    Last7Days: 'Last 7 Days',
    TopProducts: 'Top Products',
    Enabled: 'Enabled',
    Disabled: 'Disabled',
    
    // Attendance
    Attendance: 'Attendance',
    AttendanceTracker: 'Attendance Tracker',
    RecordAttendance: 'Record Attendance',
    Employee: 'Employee',
    Date: 'Date',
    Status: 'Status',
    Present: 'Present',
    Absent: 'Absent',
    RegularHours: 'Regular Hours',
    OvertimeHours: 'Overtime Hours',
    CheckIn: 'Check In',
    CheckOut: 'Check Out',
    Notes: 'Notes',
    NotesOptional: 'Notes (Optional)',
    DailyPay: 'Daily Pay Preview',
    RecentRecords: 'Recent Records',
    CalculatedAutomatically: 'Calculated automatically',
    
    // Restock
    RestockReport: 'Restock Report',
    RestockProduct: 'Restock Product',
    QuantityAdded: 'Quantity Added',
    StockBefore: 'Stock Before',
    StockAfter: 'Stock After',
    RestockedBy: 'Restocked By',
    FilterByProduct: 'Filter by Product',
    FilterByUser: 'Filter by User',
    
    // Common
    Back: 'Back',
    BackToShop: 'Back to Shop',
    BackToAdmin: 'Back to Admin',
    Loading: 'Loading...',
    Save: 'Save',
    Cancel: 'Cancel',
    Delete: 'Delete',
    Edit: 'Edit',
    Search: 'Search',
    NoRecords: 'No records found',
    AttendanceAlreadyExists: 'Attendance record already exists for this employee on this date',
    FailedToRecordAttendance: 'Failed to record attendance',
    AttendanceRecordExists: 'Attendance record already exists for this employee on',
  },
  ar: {
    // Authentication
    Login: 'تسجيل الدخول',
    Username: 'اسم المستخدم',
    Password: 'كلمة المرور',
    Logout: 'تسجيل الخروج',
    LoggedInAs: 'تم تسجيل الدخول ك',
    
    // Shop
    Shop: 'المتجر',
    Cart: 'السلة',
    Checkout: 'الدفع',
    Total: 'المجموع',
    Add: 'إضافة',
    BarcodeSearch: 'البarcode / البحث',
    ScanBarcode: 'امسح البarcode أو ابحث...',
    Products: 'المنتجات',
    Quantity: 'الكمية',
    Stock: 'المخزون',
    CartEmpty: 'السلة فارغة',
    ConfirmCheckout: 'الدفع',
    OrderPlaced: 'تم إتمام الطلب بنجاح!',
    PrintReceipt: 'طباعة الإيصال',
    
    // Admin
    AdminPanel: 'لوحة التحكم',
    ReportsInsights: 'التقارير والرؤى',
    Users: 'المستخدمون',
    UserManagement: 'إدارة المستخدمين',
    TotalSales: 'إجمالي المبيعات',
    TotalOrders: 'إجمالي الطلبات',
    AverageOrderValue: 'متوسط قيمة الطلب',
    Last7Days: 'آخر 7 أيام',
    TopProducts: 'أفضل المنتجات',
    Enabled: 'مفعل',
    Disabled: 'معطل',
    
    // Attendance
    Attendance: 'الحضور',
    AttendanceTracker: 'تتبع الحضور',
    RecordAttendance: 'تسجيل الحضور',
    Employee: 'الموظف',
    Date: 'التاريخ',
    Status: 'الحالة',
    Present: 'حاضر',
    Absent: 'غائب',
    RegularHours: 'ساعات العمل',
    OvertimeHours: 'ساعات إضافية',
    CheckIn: 'وقت الدخول',
    CheckOut: 'وقت الخروج',
    Notes: 'ملاحظات',
    NotesOptional: 'ملاحظات (اختياري)',
    DailyPay: 'معاينة الراتب اليومي',
    RecentRecords: 'السجلات الأخيرة',
    CalculatedAutomatically: 'محسوب تلقائياً',
    
    // Restock
    RestockReport: 'تقرير إعادة التخزين',
    RestockProduct: 'إعادة تخزين المنتج',
    QuantityAdded: 'الكمية المضافة',
    StockBefore: 'المخزون قبل',
    StockAfter: 'المخزون بعد',
    RestockedBy: 'تم إعادة التخزين بواسطة',
    FilterByProduct: 'تصفية حسب المنتج',
    FilterByUser: 'تصفية حسب المستخدم',
    
    // Common
    Back: 'رجوع',
    BackToShop: 'العودة إلى المتجر',
    BackToAdmin: 'العودة إلى لوحة التحكم',
    Loading: 'جاري التحميل...',
    Save: 'حفظ',
    Cancel: 'إلغاء',
    Delete: 'حذف',
    Edit: 'تعديل',
    Search: 'بحث',
    NoRecords: 'لا توجد سجلات',
    AttendanceAlreadyExists: 'سجل الحضور موجود بالفعل لهذا الموظف في هذا التاريخ',
    FailedToRecordAttendance: 'فشل في تسجيل الحضور',
    AttendanceRecordExists: 'سجل الحضور موجود بالفعل لهذا الموظف في',
  },
}

const LocalizationContext = createContext<LocalizationContextType | undefined>(undefined)

export function LocalizationProvider({ children }: { children: React.ReactNode }) {
  const [language, setLanguageState] = useState<Language>('en')

  useEffect(() => {
    const saved = localStorage.getItem('language') as Language
    if (saved) {
      setLanguageState(saved)
    }
  }, [])

  const setLanguage = (lang: Language) => {
    setLanguageState(lang)
    localStorage.setItem('language', lang)
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr'
    document.documentElement.lang = lang
  }

  useEffect(() => {
    document.documentElement.dir = language === 'ar' ? 'rtl' : 'ltr'
    document.documentElement.lang = language
  }, [language])

  const t = (key: string): string => {
    return translations[language][key] || key
  }

  return (
    <LocalizationContext.Provider
      value={{
        language,
        setLanguage,
        isRTL: language === 'ar',
        t,
      }}
    >
      {children}
    </LocalizationContext.Provider>
  )
}

export function useLocalization() {
  const context = useContext(LocalizationContext)
  if (context === undefined) {
    throw new Error('useLocalization must be used within a LocalizationProvider')
  }
  return context
}

