using SweetShopMa.Services;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class AttendancePage : ContentPage
{
    private readonly AdminViewModel _viewModel;
    private readonly LocalizationService _localizationService;

    public AttendancePage(AdminViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _localizationService = localizationService;
        BindingContext = _viewModel;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateLocalizedStrings();
        UpdateRTL();
        
        try
        {
            await _viewModel.LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AttendancePage.OnAppearing: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            // Don't show error to user, just log it
        }
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("AttendanceTracker");
        if (AttendanceTrackerTitleLabel != null)
            AttendanceTrackerTitleLabel.Text = _localizationService.GetString("AttendanceTracker");
        if (AttendanceBackButton != null)
            AttendanceBackButton.Text = _localizationService.GetString("BackButton");
        if (EmployeePicker != null)
            EmployeePicker.Title = _localizationService.GetString("Employee");
        if (StatusPicker != null)
            StatusPicker.Title = _localizationService.GetString("Status");
        if (NotesEntry != null)
            NotesEntry.Placeholder = _localizationService.GetString("NotesOptional");
        if (RecordAttendanceButton != null)
            RecordAttendanceButton.Text = _localizationService.GetString("RecordAttendanceButton");
        if (MonthlySummaryLabel != null)
            MonthlySummaryLabel.Text = _localizationService.GetString("MonthlyAttendanceSummary");
        if (MonthLabel != null)
            MonthLabel.Text = _localizationService.GetString("MonthLabel");
        if (AttendanceCalendarLabel != null)
            AttendanceCalendarLabel.Text = _localizationService.GetString("AttendanceCalendar");
        
        // Add/Edit Attendance Section
        if (AddEditAttendanceLabel != null)
            AddEditAttendanceLabel.Text = _localizationService.GetString("AddEditAttendance");
        
        // Statistics Section
        if (StatisticsLabel != null)
            StatisticsLabel.Text = _localizationService.GetString("Statistics");
        if (PresentLabel != null)
            PresentLabel.Text = _localizationService.GetString("Present");
        if (AbsentLabel != null)
            AbsentLabel.Text = _localizationService.GetString("Absent");
        if (OvertimeEntriesLabel != null)
            OvertimeEntriesLabel.Text = _localizationService.GetString("OvertimeEntries");
        if (RegularHoursLabel != null)
            RegularHoursLabel.Text = _localizationService.GetString("RegularHours");
        if (OvertimeHoursLabel != null)
            OvertimeHoursLabel.Text = _localizationService.GetString("OvertimeHours");
        if (Payroll30dLabel != null)
            Payroll30dLabel.Text = _localizationService.GetString("Payroll30d");
        
        // Enhanced Statistics Section
        if (EnhancedStatisticsLabel != null)
            EnhancedStatisticsLabel.Text = _localizationService.GetString("EnhancedStatistics");
        if (AvgHoursPerDayLabel != null)
            AvgHoursPerDayLabel.Text = _localizationService.GetString("AvgHoursPerDay");
        if (ConsecutiveDaysLabel != null)
            ConsecutiveDaysLabel.Text = _localizationService.GetString("ConsecutiveDays");
        if (MonthlyComparisonLabel != null)
            MonthlyComparisonLabel.Text = _localizationService.GetString("MonthlyComparison");
        
        // Filters Section
        if (FiltersAndSearchLabel != null)
            FiltersAndSearchLabel.Text = _localizationService.GetString("FiltersAndSearch");
        if (SearchByNameOrNotesEntry != null)
            SearchByNameOrNotesEntry.Placeholder = _localizationService.GetString("SearchByNameOrNotes");
        if (FilterStatusPicker != null)
            FilterStatusPicker.Title = _localizationService.GetString("Status");
        if (FilterOvertimePicker != null)
            FilterOvertimePicker.Title = _localizationService.GetString("Overtime");
        if (ApplyFiltersButton != null)
            ApplyFiltersButton.Text = _localizationService.GetString("ApplyFilters");
        if (ClearFiltersButton != null)
            ClearFiltersButton.Text = _localizationService.GetString("ClearFilters");
        
        // Records List Section
        if (AttendanceRecordsLabel != null)
            AttendanceRecordsLabel.Text = _localizationService.GetString("AttendanceRecords");
        if (DeleteSelectedButton != null)
            DeleteSelectedButton.Text = _localizationService.GetString("DeleteSelected");
        if (ClearSelectionButton != null)
            ClearSelectionButton.Text = _localizationService.GetString("ClearSelection");
        if (ExportToExcelButton != null)
            ExportToExcelButton.Text = _localizationService.GetString("ExportToExcel");
        if (ExportToPDFButton != null)
            ExportToPDFButton.Text = _localizationService.GetString("ExportToPDF");
        
        // Monthly Summary Section
        if (EmployeeExpensesLabel != null)
            EmployeeExpensesLabel.Text = _localizationService.GetString("EmployeeExpenses");
        if (ExportPdfButton != null)
            ExportPdfButton.Text = _localizationService.GetString("ExportPayrollPDF");
        
        // Calendar Section
        if (SunLabel != null)
            SunLabel.Text = _localizationService.GetString("Sun");
        if (MonLabel != null)
            MonLabel.Text = _localizationService.GetString("Mon");
        if (TueLabel != null)
            TueLabel.Text = _localizationService.GetString("Tue");
        if (WedLabel != null)
            WedLabel.Text = _localizationService.GetString("Wed");
        if (ThuLabel != null)
            ThuLabel.Text = _localizationService.GetString("Thu");
        if (FriLabel != null)
            FriLabel.Text = _localizationService.GetString("Fri");
        if (SatLabel != null)
            SatLabel.Text = _localizationService.GetString("Sat");
        
        // Employee Comparison Section
        if (EmployeeComparisonLabel != null)
            EmployeeComparisonLabel.Text = _localizationService.GetString("EmployeeComparison");
        
        // Additional labels for enhanced statistics section
        if (DaysLabel != null)
            DaysLabel.Text = _localizationService.GetString("Days");
        if (HoursLabel != null)
            HoursLabel.Text = _localizationService.GetString("Hours");
        if (PayrollLabel != null)
            PayrollLabel.Text = _localizationService.GetString("PayrollColon");
        
        // Update Attendance button
        if (UpdateAttendanceButton != null)
            UpdateAttendanceButton.Text = _localizationService.GetString("UpdateAttendance");
        
        // Export buttons
        if (ExportSelectedEmployeePayrollPdfButton != null)
            ExportSelectedEmployeePayrollPdfButton.Text = _localizationService.GetString("ExportSelectedEmployeePayrollPDF");
        
        // Expense section labels
        if (SelectedLabel != null)
            SelectedLabel.Text = _localizationService.GetString("Selected");
        if (AmountEntry != null)
            AmountEntry.Placeholder = _localizationService.GetString("Amount");
        if (CategoryEntry != null)
            CategoryEntry.Placeholder = _localizationService.GetString("Category");
        if (NotesEntryExpense != null)
            NotesEntryExpense.Placeholder = _localizationService.GetString("NotesExpense");
        if (AddExpenseButton != null)
            AddExpenseButton.Text = _localizationService.GetString("AddExpense");
        
        // Monthly summary totals labels
        if (TotalPresentLabel != null)
            TotalPresentLabel.Text = _localizationService.GetString("TotalPresent");
        if (TotalAbsentLabel != null)
            TotalAbsentLabel.Text = _localizationService.GetString("TotalAbsent");
        if (OTHoursLabel != null)
            OTHoursLabel.Text = _localizationService.GetString("OTHours");
        if (PayrollLabel2 != null)
            PayrollLabel2.Text = _localizationService.GetString("Payroll");
        if (RestPayoutLabel != null)
            RestPayoutLabel.Text = _localizationService.GetString("RestPayout");
        if (AbsenceDeductionsLabel != null)
            AbsenceDeductionsLabel.Text = _localizationService.GetString("AbsenceDeductions");
        
        // Update filter picker items
        if (FilterStatusPicker != null)
        {
            var allText = _localizationService.GetString("All");
            var presentText = _localizationService.GetString("Present");
            var absentText = _localizationService.GetString("Absent");
            
            FilterStatusPicker.ItemsSource = new List<string> { allText, presentText, absentText };
            
            // Update selected item if it exists
            if (FilterStatusPicker.SelectedItem != null)
            {
                var currentSelection = FilterStatusPicker.SelectedItem.ToString();
                if (currentSelection == "All" || currentSelection == "الكل")
                    FilterStatusPicker.SelectedItem = allText;
                else if (currentSelection == "Present" || currentSelection == "حاضر")
                    FilterStatusPicker.SelectedItem = presentText;
                else if (currentSelection == "Absent" || currentSelection == "غائب")
                    FilterStatusPicker.SelectedItem = absentText;
            }
        }
        
        // Update status picker items
        if (StatusPicker != null)
        {
            var presentText = _localizationService.GetString("Present");
            var resetText = _localizationService.GetString("Reset");
            var absentWithPermissionText = _localizationService.GetString("AbsentWithPermission");
            var absentWithoutPermissionText = _localizationService.GetString("AbsentWithoutPermission");
            
            StatusPicker.ItemsSource = new List<string> { presentText, resetText, absentWithPermissionText, absentWithoutPermissionText };
            
            // Use the ViewModel's internal key to determine selection
            if (_viewModel != null)
            {
                var key = _viewModel.SelectedAttendanceStatusKey;
                StatusPicker.SelectedItem = _localizationService.GetString(key);
            }
        }
        
        if (FilterOvertimePicker != null)
        {
            var allText = _localizationService.GetString("All");
            var withOTText = _localizationService.GetString("WithOT");
            var withoutOTText = _localizationService.GetString("WithoutOT");
            
            FilterOvertimePicker.ItemsSource = new List<string> { allText, withOTText, withoutOTText };
            
            // Update selected item if it exists
            if (FilterOvertimePicker.SelectedItem != null)
            {
                var currentSelection = FilterOvertimePicker.SelectedItem.ToString();
                if (currentSelection == "All" || currentSelection == "الكل")
                    FilterOvertimePicker.SelectedItem = allText;
                else if (currentSelection == "With OT" || currentSelection == "مع إضافي")
                    FilterOvertimePicker.SelectedItem = withOTText;
                else if (currentSelection == "Without OT" || currentSelection == "بدون إضافي")
                    FilterOvertimePicker.SelectedItem = withoutOTText;
            }
        }
        
        // Update ViewModel properties that depend on localization
        if (BindingContext is AdminViewModel adminViewModel)
        {
            adminViewModel.RefreshLocalizedProperties();
        }
    }

    private void UpdateRTL()
    {
        FlowDirection = _localizationService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        var currentLang = _localizationService.CurrentLanguage;
        var newLang = currentLang == "en" ? "ar" : "en";
        _localizationService.SetLanguage(newLang);
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (Navigation?.NavigationStack?.Count > 1)
        {
            await Navigation.PopAsync();
            return;
        }

        await Shell.Current.GoToAsync("..");
    }
}

