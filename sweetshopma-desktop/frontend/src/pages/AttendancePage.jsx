/**
 * Attendance Tracker Page
 * 
 * Main page for managing employee attendance, payroll, and expenses.
 * Ported from C# SweetShopMa AttendancePage.xaml
 */

import React, { useState, useEffect, useCallback } from 'react';
import { useApi } from '../services/apiService';
import { useAuth } from '../context/AuthContext';
import { useTranslation } from 'react-i18next';
import { useAttendanceRecords, useAttendanceExpenses, useAttendanceSummaries } from '../hooks/useApiData';
import LoadingSpinner from '../components/common/LoadingSpinner';

// Attendance statuses
var ATTENDANCE_STATUSES = [
    { key: 'Present', label: 'present' },
    { key: 'Reset', label: 'reset' },
    { key: 'AbsentWithPermission', label: 'absent_with_permission' },
    { key: 'AbsentWithoutPermission', label: 'absent_without_permission' }
];

// Expense categories
var EXPENSE_CATEGORIES = [
    { key: 'General', label: 'general' },
    { key: 'Supplies', label: 'supplies' },
    { key: 'Travel', label: 'travel' },
    { key: 'Meals', label: 'meals' },
    { key: 'Equipment', label: 'equipment' },
    { key: 'Other', label: 'other' }
];

function AttendancePage() {
    const { t, i18n } = useTranslation();
    var isAuthenticated = useAuth().isAuthenticated;
    var api = useApi();
    var isReady = api.isReady;

    // State for users list
    var usersState = React.useState([]);
    var users = usersState[0];
    var setUsers = usersState[1];

    // Fetch users on mount
    React.useEffect(function () {
        if (isReady) {
            fetchUsers();
            fetchSettings();
        }
    }, [isReady]);

    async function fetchSettings() {
        try {
            var settings = await api.getShopSettings();
            if (settings && !settings.error) {
                setShopSettings(settings);
                setWorkToRestRatio(settings.work_to_rest_ratio || 6);
            }
        } catch (e) {
            console.error('[AttendancePage] Error fetching settings:', e);
        }
    }

    async function fetchUsers() {
        try {
            console.log('[AttendancePage] Fetching employees for attendance...');
            console.log('[AttendancePage] User role:', localStorage.getItem('userRole') || 'unknown');
            var response = await api.getEmployeesForAttendance();
            console.log('[AttendancePage] Employees response:', response);

            if (response && Array.isArray(response)) {
                setUsers(response);
                console.log('[AttendancePage] ✓ Successfully loaded ' + response.length + ' employees');
            } else if (response && response.results && Array.isArray(response.results)) {
                setUsers(response.results);
                console.log('[AttendancePage] ✓ Successfully loaded ' + response.results.length + ' employees');
            } else if (response && response.error) {
                console.error('[AttendancePage] API returned error:', response.error);
                console.error('[AttendancePage] Error code:', response.code);
                setUsers([]);
            } else {
                console.warn('[AttendancePage] No employees found');
                setUsers([]);
            }
        } catch (e) {
            console.error('[AttendancePage] Error fetching employees:', e);
            console.error('[AttendancePage] Error message:', e.message);
            // Fallback to empty list
            setUsers([]);
        }
    }

    // State for selected employee and filters
    var selectedUserState = React.useState(null);
    var selectedUser = selectedUserState[0];
    var setSelectedUser = selectedUserState[1];

    var filterStartDateState = React.useState(
        new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0]
    );
    var filterStartDate = filterStartDateState[0];
    var setFilterStartDate = filterStartDateState[1];

    var filterEndDateState = React.useState(
        new Date().toISOString().split('T')[0]
    );
    var filterEndDate = filterEndDateState[0];
    var setFilterEndDate = filterEndDateState[1];

    var searchTextState = React.useState('');
    var searchText = searchTextState[0];
    var setSearchText = searchTextState[1];

    var filterStatusState = React.useState('All');
    var filterStatus = filterStatusState[0];
    var setFilterStatus = filterStatusState[1];

    // State for add/edit form
    var attendanceDateState = React.useState(new Date().toISOString().split('T')[0]);
    var attendanceDate = attendanceDateState[0];
    var setAttendanceDate = attendanceDateState[1];

    var selectedStatusState = React.useState('Present');
    var selectedStatus = selectedStatusState[0];
    var setSelectedStatus = selectedStatusState[1];

    var checkInTimeState = React.useState('08:00');
    var checkInTime = checkInTimeState[0];
    var setCheckInTime = checkInTimeState[1];

    var checkOutTimeState = React.useState('16:00');
    var checkOutTime = checkOutTimeState[0];
    var setCheckOutTime = checkOutTimeState[1];

    var attendanceNotesState = React.useState('');
    var attendanceNotes = attendanceNotesState[0];
    var setAttendanceNotes = attendanceNotesState[1];

    var isSubmittingState = React.useState(false);
    var isSubmitting = isSubmittingState[0];
    var setIsSubmitting = isSubmittingState[1];

    var statusMessageState = React.useState(null);
    var statusMessage = statusMessageState[0];
    var setStatusMessage = statusMessageState[1];

    var isErrorStatusState = React.useState(false);
    var isErrorStatus = isErrorStatusState[0];
    var isErrorStatus = isErrorStatusState[0];
    var setIsErrorStatus = isErrorStatusState[1];

    // State for Shop Settings
    var shopSettingsState = React.useState(null);
    var shopSettings = shopSettingsState[0];
    var setShopSettings = shopSettingsState[1];

    var workToRestRatioState = React.useState(6);
    var workToRestRatio = workToRestRatioState[0];
    var setWorkToRestRatio = workToRestRatioState[1];

    var isSettingsExpandedState = React.useState(false);
    var isSettingsExpanded = isSettingsExpandedState[0];
    var setIsSettingsExpanded = isSettingsExpandedState[1];

    var isExportingState = React.useState(false);
    var isExporting = isExportingState[0];
    var setIsExporting = isExportingState[1];

    // State for editing
    var isEditingState = React.useState(false);
    var isEditing = isEditingState[0];
    var setIsEditing = isEditingState[1];

    var editingRecordIdState = React.useState(null);
    var editingRecordId = editingRecordIdState[0];
    var setEditingRecordId = editingRecordIdState[1];

    // State for expense form
    var expenseDateState = React.useState(new Date().toISOString().split('T')[0]);
    var expenseDate = expenseDateState[0];
    var setExpenseDate = expenseDateState[1];

    var expenseAmountState = React.useState('');
    var expenseAmount = expenseAmountState[0];
    var setExpenseAmount = expenseAmountState[1];

    var expenseCategoryState = React.useState('General');
    var expenseCategory = expenseCategoryState[0];
    var setExpenseCategory = expenseCategoryState[1];

    var expenseNotesState = React.useState('');
    var expenseNotes = expenseNotesState[0];
    var setExpenseNotes = expenseNotesState[1];

    // State for bulk add
    var bulkStartDateState = React.useState(new Date().toISOString().split('T')[0]);
    var bulkStartDate = bulkStartDateState[0];
    var setBulkStartDate = bulkStartDateState[1];

    var bulkEndDateState = React.useState(new Date().toISOString().split('T')[0]);
    var bulkEndDate = bulkEndDateState[0];
    var setBulkEndDate = bulkEndDateState[1];

    var skipWeekendsState = React.useState(true);
    var skipWeekends = skipWeekendsState[0];
    var setSkipWeekends = skipWeekendsState[1];

    // State for current month
    var currentMonthState = React.useState(
        new Date().toISOString().slice(0, 7)
    );
    var currentMonth = currentMonthState[0];
    var setCurrentMonth = currentMonthState[1];

    // Update filters when month changes
    React.useEffect(function () {
        if (!currentMonth) return;

        var parts = currentMonth.split('-');
        var year = parseInt(parts[0]);
        var month = parseInt(parts[1]) - 1; // 0-indexed

        // First day of month
        var firstDay = new Date(year, month, 1);
        // Handle timezone offset to ensure we get local YYYY-MM-DD
        var firstDayStr = new Date(firstDay.getTime() - (firstDay.getTimezoneOffset() * 60000))
            .toISOString().split('T')[0];

        // Last day of month
        var lastDay = new Date(year, month + 1, 0);
        var lastDayStr = new Date(lastDay.getTime() - (lastDay.getTimezoneOffset() * 60000))
            .toISOString().split('T')[0];

        console.log('[AttendancePage] Month changed to ' + currentMonth + ', updating filters: ' + firstDayStr + ' to ' + lastDayStr);
        setFilterStartDate(firstDayStr);
        setFilterEndDate(lastDayStr);
    }, [currentMonth]);

    // State for sections visibility
    var isAddEditExpandedState = React.useState(true);
    var isAddEditExpanded = isAddEditExpandedState[0];
    var setIsAddEditExpanded = isAddEditExpandedState[1];

    var isBulkAddExpandedState = React.useState(false);
    var isBulkAddExpanded = isBulkAddExpandedState[0];
    var setIsBulkAddExpanded = isBulkAddExpandedState[1];

    var isStatisticsExpandedState = React.useState(true);
    var isStatisticsExpanded = isStatisticsExpandedState[0];
    var setIsStatisticsExpanded = isStatisticsExpandedState[1];

    var isRecordsExpandedState = React.useState(true);
    var isRecordsExpanded = isRecordsExpandedState[0];
    var setIsRecordsExpanded = isRecordsExpandedState[1];

    var isMonthlySummaryExpandedState = React.useState(true);
    var isMonthlySummaryExpanded = isMonthlySummaryExpandedState[0];
    var setIsMonthlySummaryExpanded = isMonthlySummaryExpandedState[1];

    var isCalendarExpandedState = React.useState(true);
    var isCalendarExpanded = isCalendarExpandedState[0];
    var setIsCalendarExpanded = isCalendarExpandedState[1];

    var isExpensesExpandedState = React.useState(true);
    var isExpensesExpanded = isExpensesExpandedState[0];
    var setIsExpensesExpanded = isExpensesExpandedState[1];

    var isFilteringExpandedState = React.useState(true);
    var isFilteringExpanded = isFilteringExpandedState[0];
    var setIsFilteringExpanded = isFilteringExpandedState[1];

    // API functions
    var getAttendanceRecords = api.getAttendanceRecords;
    var createAttendanceRecord = api.createAttendanceRecord;
    var bulkCreateAttendance = api.bulkCreateAttendance;
    var deleteAttendanceRecord = api.deleteAttendanceRecord;
    var getAttendanceSummaries = api.getAttendanceSummaries;
    var getAttendanceExpenses = api.getAttendanceExpenses;
    var createAttendanceExpense = api.createAttendanceExpense;
    var deleteAttendanceExpense = api.deleteAttendanceExpense;
    var checkAttendanceDuplicate = api.checkAttendanceDuplicate;
    var updateAttendanceRecord = api.updateAttendanceRecord;

    // Fetch attendance records
    var attendanceRecordsResult = useAttendanceRecords({
        user: selectedUser ? selectedUser.id : null,
        start_date: filterStartDate,
        end_date: filterEndDate,
        status: filterStatus !== 'All' ? filterStatus : null,
        search: searchText || null
    });
    var attendanceRecords = attendanceRecordsResult.data;
    var recordsLoading = attendanceRecordsResult.loading;
    var recordsError = attendanceRecordsResult.error;
    var refetchRecords = attendanceRecordsResult.refetch;

    // Fetch monthly summaries
    var summariesResult = useAttendanceSummaries(currentMonth, selectedUser ? selectedUser.id : null);
    var monthlySummaries = summariesResult.data;
    var summariesLoading = summariesResult.loading;
    var refetchSummaries = summariesResult.refetch;

    // Fetch expenses
    var expensesResult = useAttendanceExpenses({
        user: selectedUser ? selectedUser.id : null,
        start_date: filterStartDate,
        end_date: filterEndDate
    });
    var expenses = expensesResult.data;
    var expensesLoading = expensesResult.loading;
    var refetchExpenses = expensesResult.refetch;

    // Calculate statistics
    var calculateStatistics = React.useCallback(function () {
        if (!attendanceRecords) return null;

        var presentCount = 0;
        var absentCount = 0;
        var overtimeCount = 0;
        var totalRegularHours = 0;
        var totalOvertimeHours = 0;
        var totalPayroll = 0;

        for (var i = 0; i < attendanceRecords.length; i++) {
            var r = attendanceRecords[i];
            if (r.is_present) {
                presentCount++;
            } else {
                absentCount++;
            }
            if (r.overtime_hours > 0) {
                overtimeCount++;
            }
            totalRegularHours += parseFloat(r.regular_hours || 0);
            totalOvertimeHours += parseFloat(r.overtime_hours || 0);
            totalPayroll += parseFloat(r.daily_pay || 0);
        }

        return {
            presentCount: presentCount,
            absentCount: absentCount,
            overtimeCount: overtimeCount,
            totalRegularHours: totalRegularHours,
            totalOvertimeHours: totalOvertimeHours,
            totalPayroll: totalPayroll
        };
    }, [attendanceRecords]);

    var stats = calculateStatistics();

    // Calculate regular hours based on check-in/out times
    function calculateRegularHours() {
        if (selectedStatus === 'Present' || selectedStatus === 'Reset') {
            var inTime = new Date('2000-01-01T' + checkInTime);
            var outTime = new Date('2000-01-01T' + checkOutTime);
            var hours = (outTime - inTime) / (1000 * 60 * 60);
            return Math.min(hours, 8).toFixed(2);
        }
        return '0.00';
    }

    // Calculate overtime hours
    function calculateOvertimeHours() {
        if (selectedStatus === 'Present' || selectedStatus === 'Reset') {
            var inTime = new Date('2000-01-01T' + checkInTime);
            var outTime = new Date('2000-01-01T' + checkOutTime);
            var hours = (outTime - inTime) / (1000 * 60 * 60);
            return Math.max(hours - 8, 0).toFixed(2);
        }
        return '0.00';
    }

    // Calculate daily pay
    function calculateDailyPay() {
        return '0.00';
    }

    // Get absence permission type
    function getAbsencePermissionType(status) {
        switch (status) {
            case 'AbsentWithPermission': return 'WithPermission';
            case 'AbsentWithoutPermission': return 'WithoutPermission';
            case 'Reset': return 'Reset';
            default: return 'None';
        }
    }

    // Show status message
    function showStatus(message, isError) {
        setStatusMessage(message);
        setIsErrorStatus(isError);
        setTimeout(function () { setStatusMessage(null); }, 3000);
    }

    // Check if record already exists for this user and date using API
    async function checkDuplicateAttendance(userId, date) {
        console.log('[checkDuplicateAttendance] Checking for duplicate via API...');
        console.log('[checkDuplicateAttendance] User ID:', userId, 'Date:', date);

        try {
            var result = await checkAttendanceDuplicate(userId, date);
            console.log('[checkDuplicateAttendance] API result:', result);

            if (result && result.exists) {
                console.log('[checkDuplicateAttendance] Duplicate found:', result.record);
                return result.record;
            }

            console.log('[checkDuplicateAttendance] No duplicate found');
            return null;
        } catch (error) {
            console.error('[checkDuplicateAttendance] Error checking duplicate:', error);
            // Fallback to local check
            console.log('[checkDuplicateAttendance] Falling back to local check...');

            if (!attendanceRecords || !Array.isArray(attendanceRecords)) {
                return null;
            }

            for (var i = 0; i < attendanceRecords.length; i++) {
                var record = attendanceRecords[i];
                // Handle type mismatches
                var recordUserId = typeof record.user === 'string' ? parseInt(record.user) : record.user;
                var selectedUserIdNum = typeof userId === 'string' ? parseInt(userId) : userId;

                if (recordUserId === selectedUserIdNum && record.date === date) {
                    return record;
                }
            }

            return null;
        }
    }

    // Handle attendance submission
    var handleSubmitAttendance = async function (e) {
        e.preventDefault();

        if (!selectedUser) {
            showStatus(t('select_employee_alert'), true);
            return;
        }

        // Check for duplicate only if NOT editing, or if editing and date changed (which we might not support yet, but good to be safe)
        // For simplicity, if editing, we skip duplicate check on the assumption we are updating the same record
        if (!isEditing) {
            var existingRecord = await checkDuplicateAttendance(selectedUser.id, attendanceDate);

            if (existingRecord) {
                showStatus(t('attendance_recorded_already', { name: (selectedUser.username || selectedUser.email || t('employee')), date: attendanceDate }) || ('Attendance already recorded for ' + (selectedUser.username || selectedUser.email || 'this employee') + ' on ' + attendanceDate), true);
                return;
            }
        }

        setIsSubmitting(true);

        try {
            var data = {
                user: selectedUser.id,
                date: attendanceDate,
                status: selectedStatus,
                is_present: selectedStatus === 'Present',
                regular_hours: calculateRegularHours(),
                overtime_hours: calculateOvertimeHours(),
                daily_pay: calculateDailyPay(),
                notes: attendanceNotes,
                absence_permission_type: getAbsencePermissionType(selectedStatus)
            };

            if (isEditing) {
                console.log('[handleSubmitAttendance] Updating attendance record:', editingRecordId);
                await updateAttendanceRecord(editingRecordId, data);
                showStatus(t('success_updated'), false);
                setIsEditing(false);
                setEditingRecordId(null);
            } else {
                console.log('[handleSubmitAttendance] Creating attendance record...');
                await createAttendanceRecord(data);
                showStatus(t('success_saved'), false);
            }

            refetchRecords();
            refetchSummaries();

            setAttendanceNotes('');
            // Reset status to default if creating, but maybe keep if editing? 
            // Better to reset to clear the form visually
            if (isEditing) {
                setSelectedStatus('Present');
                setAttendanceDate(new Date().toISOString().split('T')[0]);
            }

        } catch (error) {
            console.error('[handleSubmitAttendance] Error saving attendance:', error);
            showStatus(t('error_saving_attendance') + ': ' + error.message, true);
        } finally {
            setIsSubmitting(false);
        }
    };

    // Handle edit click
    var handleEdit = function (record) {
        console.log('Editing record:', record);
        setIsEditing(true);
        setEditingRecordId(record.id);

        // Populate form
        setAttendanceDate(record.date);
        setSelectedStatus(record.status);
        setAttendanceNotes(record.notes || '');

        // Try to infer times if available, otherwise keep default or calculate
        // For now, we rely on the calculateRegularHours logic which uses the time inputs

        setIsAddEditExpanded(true);

        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    // Cancel edit
    var handleCancelEdit = function () {
        setIsEditing(false);
        setEditingRecordId(null);
        setAttendanceDate(new Date().toISOString().split('T')[0]);
        setAttendanceNotes('');
        setSelectedStatus('Present');
    };

    // Handle bulk attendance submission
    var handleBulkSubmit = async function (e) {
        e.preventDefault();

        if (!selectedUser) {
            showStatus(t('select_employee_alert'), true);
            return;
        }

        if (new Date(bulkStartDate) > new Date(bulkEndDate)) {
            showStatus(t('error_start_date_after_end'), true);
            return;
        }

        setIsSubmitting(true);

        try {
            var data = {
                user: selectedUser.id,
                start_date: bulkStartDate,
                end_date: bulkEndDate,
                status: selectedStatus,
                skip_weekends: skipWeekends,
                notes: attendanceNotes
            };

            console.log('[handleBulkSubmit] Creating bulk attendance...', data);
            var response = await bulkCreateAttendance(data);

            if (response.skipped_dates && response.skipped_dates.length > 0) {
                showStatus(t('bulk_skipped_message', { created: response.created_count, skipped: response.skipped_dates.length }), true);
            } else {
                showStatus(t('bulk_created_message', { count: response.created_count }), false);
            }

            refetchRecords();
            refetchSummaries();
            setAttendanceNotes('');

        } catch (error) {
            console.error('[handleBulkSubmit] Error:', error);
            showStatus(t('error_creating_bulk') + ': ' + error.message, true);
        } finally {
            setIsSubmitting(false);
        }
    };

    // Handle expense submission
    var handleSubmitExpense = async function (e) {
        e.preventDefault();

        if (!selectedUser) {
            showStatus(t('select_employee_alert'), true);
            return;
        }

        if (!expenseAmount || parseFloat(expenseAmount) <= 0) {
            showStatus(t('enter_valid_amount'), true);
            return;
        }

        setIsSubmitting(true);

        try {
            var data = {
                user: selectedUser.id,
                expense_date: expenseDate,
                amount: parseFloat(expenseAmount),
                category: expenseCategory,
                notes: expenseNotes
            };

            await createAttendanceExpense(data);
            showStatus(t('expense_added_success'), false);
            refetchExpenses();
            refetchSummaries();

            setExpenseAmount('');
            setExpenseCategory('General');
            setExpenseNotes('');
        } catch (error) {
            showStatus(t('error_adding_expense') + ': ' + error.message, true);
        } finally {
            setIsSubmitting(false);
        }
    };

    // Delete attendance record
    var handleDeleteRecord = async function (record) {
        if (!window.confirm(t('delete_confirm_record'))) {
            return;
        }

        try {
            await deleteAttendanceRecord(record.id);
            showStatus(t('attendance_deleted_success'), false);
            refetchRecords();
            refetchSummaries();
        } catch (error) {
            showStatus(t('error_deleting_record') + ': ' + error.message, true);
        }
    };

    // Delete expense
    var handleDeleteExpense = async function (expense) {
        if (!window.confirm(t('delete_confirm_expense'))) {
            return;
        }

        try {
            await deleteAttendanceExpense(expense.id);
            showStatus(t('expense_deleted_success'), false);
            refetchExpenses();
            refetchSummaries();
        } catch (error) {
            showStatus(t('error_deleting_expense') + ': ' + error.message, true);
        }
    };

    // Update Shop Settings
    var handleUpdateSettings = async function (e) {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            var data = {
                work_to_rest_ratio: parseInt(workToRestRatio)
            };
            var result = await api.updateShopSettings(data);
            if (result && !result.error) {
                setShopSettings(result);
                showStatus(t('settings_updated_success'), false);
                // Refetch summaries to reflect new ratio
                refetchSummaries();
            } else {
                throw new Error(result.error || 'Failed to update settings');
            }
        } catch (error) {
            showStatus(t('error_updating_settings') + ': ' + error.message, true);
        } finally {
            setIsSubmitting(false);
        }
    };

    // Export PDF
    var handleExportPdf = async function () {
        if (!selectedUser) {
            showStatus(t('select_employee_export'), true);
            return;
        }

        setIsExporting(true);
        try {
            const response = await api.exportAttendancePdf(selectedUser.id, currentMonth, i18n.language || 'ar');

            if (response && response.success) {
                // Show success status with file path
                setStatusMessage(`✅ Report saved to: ${response.file_path}`);
                setIsErrorStatus(false);
                // Clear message after 10 seconds
                setTimeout(function () { setStatusMessage(null); }, 10000);
            } else {
                throw new Error(response.error || 'Failed to generate PDF');
            }
        } catch (e) {
            console.error('Export failed:', e);
            showStatus(t('error_export_pdf') + ': ' + e.message, true);
        } finally {
            setIsExporting(false);
        }
    };

    // Generate calendar days
    function generateCalendarDays() {
        var days = [];
        var year = parseInt(currentMonth.split('-')[0]);
        var month = parseInt(currentMonth.split('-')[1]) - 1;
        var firstDay = new Date(year, month, 1);
        var lastDay = new Date(year, month + 1, 0);
        var startDayOfWeek = firstDay.getDay();

        // Add empty days for alignment
        for (var i = 0; i < startDayOfWeek; i++) {
            days.push({ day: '', isPlaceholder: true });
        }

        // Add days of the month
        for (var day = 1; day <= lastDay.getDate(); day++) {
            var dayStr = day.toString().padStart(2, '0');
            var dateStr = currentMonth + '-' + dayStr;
            var record = null;
            if (attendanceRecords) {
                for (var j = 0; j < attendanceRecords.length; j++) {
                    if (attendanceRecords[j].date === dateStr) {
                        record = attendanceRecords[j];
                        break;
                    }
                }
            }
            days.push({
                day: day,
                date: dateStr,
                isPlaceholder: false,
                status: record ? record.status : '',
                is_present: record ? record.is_present : false,
                record: record
            });
        }

        return days;
    }

    if (!isAuthenticated) {
        return React.createElement('div', null, t('please_login_attendance'));
    }

    if (!isReady) {
        return React.createElement(LoadingSpinner, { message: t('initializing_attendance') });
    }

    if (recordsError) {
        return React.createElement('div', {
            style: { padding: '20px', color: '#dc2626' }
        }, t('failed_load_attendance') + ': ' + recordsError);
    }

    var calendarDays = generateCalendarDays();

    // Render section helper
    function renderSection(title, isExpanded, setExpanded, children) {
        return React.createElement('div', {
            style: {
                border: '1px solid #e0e0e0',
                borderRadius: '8px',
                marginBottom: '20px',
                overflow: 'hidden'
            }
        },
            React.createElement('div', {
                style: {
                    padding: '12px',
                    backgroundColor: '#f0f0f0',
                    borderBottom: '1px solid #e0e0e0',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center'
                }
            },
                React.createElement('h2', {
                    style: { fontSize: '16px', fontWeight: 'bold', margin: 0 }
                }, t(title.toLowerCase().replace(/ /g, '_')) || title),
                React.createElement('button', {
                    onClick: function () { setExpanded(!isExpanded); },
                    style: { background: 'none', border: 'none', cursor: 'pointer', fontSize: '18px' }
                }, isExpanded ? '▼' : '▶')
            ),
            isExpanded && React.createElement('div', { style: { padding: '16px', backgroundColor: 'white' } }, children)
        );
    }

    // Render stat card
    function renderStatCard(label, value) {
        return React.createElement('div', {
            style: {
                padding: '12px',
                border: '1px solid #e0e0e0',
                borderRadius: '4px',
                textAlign: 'center'
            }
        },
            React.createElement('div', {
                style: { fontSize: '12px', color: '#666' }
            }, label),
            React.createElement('div', {
                style: { fontSize: '24px', fontWeight: 'bold' }
            }, value)
        );
    }

    return React.createElement('div', { className: 'attendance-page', style: { padding: '20px' } },
        // Header
        React.createElement('div', { style: { marginBottom: '20px' } },
            React.createElement('h1', {
                style: { fontSize: '24px', fontWeight: 'bold', margin: 0 }
            }, t('attendance_tracker')),
        ),

        // Main Actions Bar
        React.createElement('div', {
            style: {
                padding: '16px',
                backgroundColor: 'white',
                borderRadius: '8px',
                boxShadow: '0 2px 4px rgba(0,0,0,0.05)',
                marginBottom: '20px',
                display: 'flex',
                flexWrap: 'wrap',
                gap: '16px',
                alignItems: 'end',
                border: '1px solid #e0e0e0'
            }
        },
            // Global Employee Selector
            React.createElement('div', { style: { flexGrow: 1, minWidth: '200px' } },
                React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: 'bold' } }, t('select_employee')),
                React.createElement('select', {
                    value: selectedUser ? selectedUser.id : '',
                    onChange: function (e) {
                        var userId = parseInt(e.target.value);
                        var selectedUserObj = users.find(function (u) { return u.id === userId; });
                        setSelectedUser(selectedUserObj || null);
                    },
                    style: { width: '100%', padding: '10px', borderRadius: '4px', border: '1px solid #ddd', backgroundColor: '#fff' }
                },
                    React.createElement('option', { value: '' }, t('all_employees')),
                    users.map(function (user) {
                        return React.createElement('option', {
                            key: user.id,
                            value: user.id
                        }, user.username || user.email || t('user') + ' ' + user.id);
                    })
                )
            ),

            // Global Month Selector
            React.createElement('div', { style: { minWidth: '150px' } },
                React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: 'bold' } }, t('month')),
                React.createElement('div', { style: { display: 'flex', gap: '4px' } },
                    React.createElement('button', {
                        onClick: function () {
                            // currentMonth expected to be YYYY-MM
                            var parts = currentMonth.split('-');
                            var d = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1 - 1, 1);
                            var m = d.getMonth() + 1;
                            var y = d.getFullYear();
                            setCurrentMonth(y + '-' + m.toString().padStart(2, '0'));
                        },
                        style: { padding: '10px', cursor: 'pointer', border: '1px solid #ddd', borderRadius: '4px' }
                    }, '◀'),
                    React.createElement('input', {
                        type: 'month',
                        value: currentMonth,
                        onChange: function (e) { setCurrentMonth(e.target.value); },
                        style: { padding: '10px', borderRadius: '4px', border: '1px solid #ddd' }
                    }),
                    React.createElement('button', {
                        onClick: function () {
                            var parts = currentMonth.split('-');
                            var d = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1 + 1, 1);
                            var m = d.getMonth() + 1;
                            var y = d.getFullYear();
                            setCurrentMonth(y + '-' + m.toString().padStart(2, '0'));
                        },
                        style: { padding: '10px', cursor: 'pointer', border: '1px solid #ddd', borderRadius: '4px' }
                    }, '▶')
                )
            ),

            // Export Button
            React.createElement('div', null,
                React.createElement('button', {
                    onClick: handleExportPdf,
                    disabled: isExporting || !selectedUser,
                    title: !selectedUser ? t('select_employee_export') : t('export_pdf'),
                    style: {
                        padding: '10px 20px',
                        backgroundColor: !selectedUser ? '#9ca3af' : '#2563eb', // Blue
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: (isExporting || !selectedUser) ? 'not-allowed' : 'pointer',
                        opacity: isExporting ? 0.7 : 1,
                        display: 'flex',
                        alignItems: 'center',
                        gap: '8px',
                        fontWeight: 'bold'
                    }
                },
                    React.createElement('span', { style: { fontSize: '18px' } }, '📄'),
                    isExporting ? t('exporting') : t('export_pdf')
                )
            )
        ),

        // Status Message
        statusMessage && React.createElement('div', {
            style: {
                padding: '10px',
                marginBottom: '20px',
                borderRadius: '4px',
                backgroundColor: isErrorStatus ? '#fee2e2' : '#dcfce7',
                color: isErrorStatus ? '#dc2626' : '#16a34a',
                fontWeight: 'bold'
            }
        }, statusMessage),

        // Attendance Settings Section
        renderSection('Attendance Settings', isSettingsExpanded, setIsSettingsExpanded,
            React.createElement('form', { onSubmit: handleUpdateSettings },
                React.createElement('div', {
                    style: { display: 'flex', alignItems: 'end', gap: '12px' }
                },
                    React.createElement('div', { style: { flexGrow: 1 } },
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('work_to_rest_ratio')),
                        React.createElement('input', {
                            type: 'number',
                            value: workToRestRatio,
                            onChange: function (e) { setWorkToRestRatio(e.target.value); },
                            min: '1',
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    React.createElement('button', {
                        type: 'submit',
                        disabled: isSubmitting,
                        style: {
                            padding: '10px 20px',
                            backgroundColor: '#4b5563',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: isSubmitting ? 'not-allowed' : 'pointer'
                        }
                    }, isSubmitting ? t('saving') : t('update_settings'))
                )
            )
        ),

        // Add/Edit Attendance Section
        renderSection('Add/Edit Attendance', isAddEditExpanded, setIsAddEditExpanded,
            React.createElement('form', { onSubmit: handleSubmitAttendance },
                React.createElement('div', {
                    style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '12px' }
                },
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('employee')),
                        React.createElement('select', {
                            value: selectedUser ? selectedUser.id : '',
                            onChange: function (e) {
                                var userId = parseInt(e.target.value);
                                var selectedUserObj = users.find(function (u) { return u.id === userId; });
                                setSelectedUser(selectedUserObj || null);
                            },
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        },
                            React.createElement('option', { value: '' }, t('select_employee')),
                            users.map(function (user) {
                                return React.createElement('option', {
                                    key: user.id,
                                    value: user.id
                                }, user.username || user.email || 'User ' + user.id);
                            })
                        )
                    ),
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('date')),
                        React.createElement('input', {
                            type: 'date',
                            value: attendanceDate,
                            onChange: function (e) { setAttendanceDate(e.target.value); },
                            max: new Date().toISOString().split('T')[0],
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('status')),
                        React.createElement('select', {
                            value: selectedStatus,
                            onChange: function (e) { setSelectedStatus(e.target.value); },
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        },
                            ATTENDANCE_STATUSES.map(function (s) {
                                return React.createElement('option', { key: s.key, value: s.key }, s.label);
                            })
                        )
                    ),
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('check_in')),
                        React.createElement('input', {
                            type: 'time',
                            value: checkInTime,
                            onChange: function (e) { setCheckInTime(e.target.value); },
                            disabled: selectedStatus !== 'Present' && selectedStatus !== 'Reset',
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('check_out')),
                        React.createElement('input', {
                            type: 'time',
                            value: checkOutTime,
                            onChange: function (e) { setCheckOutTime(e.target.value); },
                            disabled: selectedStatus !== 'Present' && selectedStatus !== 'Reset',
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    React.createElement('div', { style: { gridColumn: '1 / -1' } },
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('notes')),
                        React.createElement('input', {
                            type: 'text',
                            value: attendanceNotes,
                            onChange: function (e) { setAttendanceNotes(e.target.value); },
                            placeholder: t('optional_notes'),
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    )
                ),
                React.createElement('div', { style: { marginTop: '16px', display: 'flex', gap: '10px' } },
                    React.createElement('button', {
                        type: 'submit',
                        disabled: isSubmitting,
                        style: {
                            padding: '10px 20px',
                            backgroundColor: isEditing ? '#f59e0b' : '#1d7480',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: isSubmitting ? 'not-allowed' : 'pointer',
                            opacity: isSubmitting ? 0.7 : 1
                        }
                    }, isSubmitting ? t('saving') : (isEditing ? t('update_attendance') : t('record_attendance'))),

                    isEditing && React.createElement('button', {
                        type: 'button',
                        onClick: function () {
                            if (editingRecordId) {
                                handleDeleteRecord({ id: editingRecordId });
                                handleCancelEdit();
                            }
                        },
                        style: {
                            padding: '10px 20px',
                            backgroundColor: '#dc2626', // Red
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            marginRight: '10px'
                        }
                    }, t('delete')),

                    isEditing && React.createElement('button', {
                        type: 'button',
                        onClick: handleCancelEdit,
                        style: {
                            padding: '10px 20px',
                            backgroundColor: '#6b7280', // Gray (changed from Red)
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }
                    }, t('cancel'))
                )
            )
        ),

        // Bulk Add Attendance Section
        renderSection('Bulk Add Attendance', isBulkAddExpanded, setIsBulkAddExpanded,
            React.createElement('form', { onSubmit: handleBulkSubmit },
                React.createElement('div', {
                    style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '12px' }
                },
                    // Employee Selector (Reused)
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('employee')),
                        React.createElement('select', {
                            value: selectedUser ? selectedUser.id : '',
                            onChange: function (e) {
                                var userId = parseInt(e.target.value);
                                var selectedUserObj = users.find(function (u) { return u.id === userId; });
                                setSelectedUser(selectedUserObj || null);
                            },
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        },
                            React.createElement('option', { value: '' }, t('select_employee')),
                            users.map(function (user) {
                                return React.createElement('option', {
                                    key: user.id,
                                    value: user.id
                                }, user.username || user.email || 'User ' + user.id);
                            })
                        )
                    ),
                    // Start Date
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('start_date')),
                        React.createElement('input', {
                            type: 'date',
                            value: bulkStartDate,
                            onChange: function (e) { setBulkStartDate(e.target.value); },
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    // End Date
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('end_date')),
                        React.createElement('input', {
                            type: 'date',
                            value: bulkEndDate,
                            onChange: function (e) { setBulkEndDate(e.target.value); },
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    // Quick Month Selector
                    React.createElement('div', { style: { display: 'flex', alignItems: 'end' } },
                        React.createElement('button', {
                            type: 'button',
                            onClick: function () {
                                var parts = currentMonth.split('-');
                                var year = parseInt(parts[0]);
                                var month = parseInt(parts[1]) - 1;
                                var firstDay = new Date(year, month, 1);
                                var lastDay = new Date(year, month + 1, 0);

                                // NEW: Clamp to today if current month
                                var today = new Date();
                                if (today.getFullYear() === year && today.getMonth() === month) {
                                    lastDay = today;
                                }

                                var firstDayStr = new Date(firstDay.getTime() - (firstDay.getTimezoneOffset() * 60000)).toISOString().split('T')[0];
                                var lastDayStr = new Date(lastDay.getTime() - (lastDay.getTimezoneOffset() * 60000)).toISOString().split('T')[0];
                                setBulkStartDate(firstDayStr);
                                setBulkEndDate(lastDayStr);
                            },
                            style: {
                                padding: '8px 16px',
                                backgroundColor: '#f0f0f0',
                                border: '1px solid #ccc',
                                borderRadius: '4px',
                                cursor: 'pointer',
                                width: '100%'
                            }
                        }, t('fill_current_month'))
                    ),
                    // Status Selector
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('status')),
                        React.createElement('select', {
                            value: selectedStatus,
                            onChange: function (e) { setSelectedStatus(e.target.value); },
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        },
                            ATTENDANCE_STATUSES.map(function (s) {
                                return React.createElement('option', { key: s.key, value: s.key }, s.label);
                            })
                        )
                    ),
                    // Notes
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, t('notes')),
                        React.createElement('input', {
                            type: 'text',
                            value: attendanceNotes,
                            onChange: function (e) { setAttendanceNotes(e.target.value); },
                            placeholder: t('optional_notes'),
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    // Skip Weekends Checkbox
                    React.createElement('div', { style: { display: 'flex', alignItems: 'center', height: '100%' } },
                        React.createElement('label', { style: { display: 'flex', alignItems: 'center', cursor: 'pointer' } },
                            React.createElement('input', {
                                type: 'checkbox',
                                checked: skipWeekends,
                                onChange: function (e) { setSkipWeekends(e.target.checked); },
                                style: { marginRight: '8px', width: '16px', height: '16px' }
                            }),
                            t('skip_weekends')
                        )
                    )
                ),
                React.createElement('div', { style: { marginTop: '16px' } },
                    React.createElement('button', {
                        type: 'submit',
                        disabled: isSubmitting,
                        style: {
                            padding: '10px 20px',
                            backgroundColor: '#2563eb', // Different color (blue)
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: isSubmitting ? 'not-allowed' : 'pointer',
                            opacity: isSubmitting ? 0.7 : 1
                        }
                    }, isSubmitting ? t('processing') : t('add_bulk_attendance'))
                )
            )
        ),

        // Statistics Section
        stats && renderSection('Statistics', isStatisticsExpanded, setIsStatisticsExpanded,
            React.createElement('div', {
                style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '12px' }
            },
                renderStatCard(t('present'), stats.presentCount),
                renderStatCard(t('absent'), stats.absentCount),
                renderStatCard(t('overtime'), stats.overtimeCount),
                renderStatCard(t('regular_hours'), stats.totalRegularHours.toFixed(1) + 'h'),
                renderStatCard(t('ot_hours'), stats.totalOvertimeHours.toFixed(1) + 'h'),
                renderStatCard(t('payroll'), '$' + stats.totalPayroll.toFixed(2))
            ),
        ),

        // Calendar Section
        renderSection('Attendance Calendar', isCalendarExpanded, setIsCalendarExpanded,
            React.createElement('div', null,
                React.createElement('div', { style: { marginBottom: '16px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#666', fontSize: '14px' } },
                    t('showing_attendance_for', { name: (selectedUser ? (selectedUser.username || t('selected_employee')) : t('all_employees')) })
                ),
                // Calendar grid
                // Calendar grid
                React.createElement('div', {
                    style: { display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '4px', textAlign: 'center' }
                },
                    ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map(function (day) {
                        return React.createElement('div', {
                            key: day,
                            style: { padding: '8px', fontWeight: 'bold', fontSize: '12px' }
                        }, t(day.toLowerCase()));
                    }),
                    calendarDays.map(function (day, index) {
                        return React.createElement('div', {
                            key: index,
                            onClick: function () { if (day.record) handleEdit(day.record); },
                            style: {
                                padding: '12px',
                                minHeight: '60px',
                                backgroundColor: day.isPlaceholder ? 'transparent' : (day.is_present ? '#d4edda' : '#f8d7da'),
                                border: day.isPlaceholder ? 'none' : '1px solid #e0e0e0',
                                borderRadius: '4px',
                                opacity: day.isPlaceholder ? 0.3 : 1,
                                cursor: day.record ? 'pointer' : 'default'
                            }
                        },
                            React.createElement('div', { style: { fontWeight: 'bold' } }, day.day),
                            day.status && React.createElement('div', {
                                style: { fontSize: '10px', marginTop: '4px' }
                            }, day.is_present ? 'P' : 'A'),
                            // Overtime Indicator
                            day.record && parseFloat(day.record.overtime_hours) > 0 && React.createElement('div', {
                                style: {
                                    fontSize: '10px',
                                    color: '#b91c1c', // Dark red
                                    fontWeight: 'bold',
                                    marginTop: '2px'
                                }
                            }, '+' + parseFloat(day.record.overtime_hours).toFixed(1) + 'h OT')
                        );
                    })
                )
            )
        ),

        // Monthly Summary Section
        monthlySummaries && monthlySummaries.length > 0 && renderSection('Monthly Summary', isMonthlySummaryExpanded, setIsMonthlySummaryExpanded,
            React.createElement('table', { style: { width: '100%', borderCollapse: 'collapse' } },
                React.createElement('thead', null,
                    React.createElement('tr', { style: { borderBottom: '2px solid #e0e0e0' } },
                        React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, t('employee')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, t('days_present')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, t('days_absent')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, t('rest_days')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, t('base_pay')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, t('rest_pay')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, t('deductions')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, t('expenses')),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, t('net_pay'))
                    )
                ),
                React.createElement('tbody', null,
                    monthlySummaries.map(function (summary) {
                        return React.createElement('tr', { key: summary.id, style: { borderBottom: '1px solid #e0e0e0' } },
                            React.createElement('td', { style: { padding: '12px' } }, summary.user_name),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'center' } }, summary.days_present),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'center' } }, summary.days_absent),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'center', color: '#2563eb', fontWeight: 'bold' } }, summary.rest_days || 0),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'right' } },
                                '$' + parseFloat(summary.total_payroll || 0).toFixed(2)),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'right', color: '#16a34a' } },
                                '+' + '$' + parseFloat(summary.rest_day_payout || 0).toFixed(2)),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'right', color: '#dc2626' } },
                                '-' + '$' + parseFloat(summary.absence_deductions || 0).toFixed(2)),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'right' } },
                                '$' + parseFloat(summary.expenses_total || 0).toFixed(2)),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'right', fontWeight: 'bold', color: '#1d7480' } },
                                '$' + parseFloat(summary.final_payroll || 0).toFixed(2))
                        );
                    })
                )
            )
        ),

        // Attendance Records Section
        renderSection('Attendance Records', isRecordsExpanded, setIsRecordsExpanded,
            React.createElement('div', null,
                // Filtering
                React.createElement('div', {
                    style: { marginBottom: '16px', padding: '12px', backgroundColor: '#f9f9f9', borderRadius: '4px' }
                },
                    React.createElement('div', {
                        style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '12px' }
                    },
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, t('start_date')),
                            React.createElement('input', {
                                type: 'date',
                                value: filterStartDate,
                                onChange: function (e) { setFilterStartDate(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, t('end_date')),
                            React.createElement('input', {
                                type: 'date',
                                value: filterEndDate,
                                onChange: function (e) { setFilterEndDate(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, t('search')),
                            React.createElement('input', {
                                type: 'text',
                                value: searchText,
                                onChange: function (e) { setSearchText(e.target.value); },
                                placeholder: t('search_users'),
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, t('status')),
                            React.createElement('select', {
                                value: filterStatus,
                                onChange: function (e) { setFilterStatus(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            },
                                React.createElement('option', { value: 'All' }, t('all')),
                                React.createElement('option', { value: 'Present' }, t('present')),
                                React.createElement('option', { value: 'AbsentWithPermission' }, t('absent_with_permission')),
                                React.createElement('option', { value: 'AbsentWithoutPermission' }, t('absent_without_permission'))
                            )
                        )
                    )
                ),

                // Records table
                recordsLoading ?
                    React.createElement(LoadingSpinner, { message: t('loading_records') }) :
                    attendanceRecords && attendanceRecords.length > 0 ?
                        React.createElement('table', { style: { width: '100%', borderCollapse: 'collapse' } },
                            React.createElement('thead', null,
                                React.createElement('tr', { style: { borderBottom: '2px solid #e0e0e0' } },
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, t('date')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, t('employee')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, t('status')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, t('hours')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, t('pay')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, t('notes')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, t('actions'))
                                )
                            ),
                            React.createElement('tbody', null,
                                attendanceRecords.map(function (record) {
                                    return React.createElement('tr', { key: record.id, style: { borderBottom: '1px solid #e0e0e0' } },
                                        React.createElement('td', { style: { padding: '12px' } }, record.date),
                                        React.createElement('td', { style: { padding: '12px' } }, record.user_name),
                                        React.createElement('td', {
                                            style: {
                                                padding: '12px',
                                                textAlign: 'center',
                                                color: record.is_present ? '#16a34a' : '#dc2626',
                                                fontWeight: 'bold'
                                            }
                                        }, record.status),
                                        React.createElement('td', { style: { padding: '12px', textAlign: 'center' } },
                                            parseFloat(record.regular_hours || 0).toFixed(1) + 'h',
                                            parseFloat(record.overtime_hours || 0) > 0 &&
                                            React.createElement('span', { style: { color: '#dc2626' } },
                                                ' + ' + parseFloat(record.overtime_hours).toFixed(1) + 'h OT'
                                            )
                                        ),
                                        React.createElement('td', {
                                            style: { padding: '12px', textAlign: 'right', fontWeight: 'bold' }
                                        }, '$' + parseFloat(record.daily_pay || 0).toFixed(2)),
                                        React.createElement('td', { style: { padding: '12px', fontSize: '12px', color: '#666' } }, record.notes || '-'),
                                        React.createElement('td', { style: { padding: '12px', textAlign: 'center' } },
                                            React.createElement('button', {
                                                onClick: function () { handleDeleteRecord(record); },
                                                style: {
                                                    padding: '6px 12px',
                                                    backgroundColor: '#dc2626',
                                                    color: 'white',
                                                    border: 'none',
                                                    borderRadius: '4px',
                                                    cursor: 'pointer',
                                                    fontSize: '12px'
                                                }
                                            }, t('delete'))
                                        )
                                    );
                                })
                            )
                        ) :
                        React.createElement('div', {
                            style: { padding: '40px', textAlign: 'center', color: '#666' }
                        }, t('no_records_found'))
            )
        ),

        // Employee Expenses Section
        renderSection('Employee Expenses', isExpensesExpanded, setIsExpensesExpanded,
            React.createElement('div', null,
                // Add Expense Form
                React.createElement('form', { onSubmit: handleSubmitExpense, style: { marginBottom: '20px', padding: '16px', backgroundColor: '#f9f9f9', borderRadius: '4px' } },
                    React.createElement('h3', { style: { fontSize: '14px', fontWeight: 'bold', marginBottom: '12px' } }, t('add_expense')),
                    React.createElement('div', {
                        style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '12px' }
                    },
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, t('date')),
                            React.createElement('input', {
                                type: 'date',
                                value: expenseDate,
                                onChange: function (e) { setExpenseDate(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, t('amount')),
                            React.createElement('input', {
                                type: 'number',
                                step: '0.01',
                                value: expenseAmount,
                                onChange: function (e) { setExpenseAmount(e.target.value); },
                                placeholder: '0.00',
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, t('category')),
                            React.createElement('select', {
                                value: expenseCategory,
                                onChange: function (e) { setExpenseCategory(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            },
                                EXPENSE_CATEGORIES.map(function (c) {
                                    return React.createElement('option', { key: c.key, value: c.key }, t(c.label));
                                })
                            )
                        )
                    ),
                    React.createElement('div', { style: { marginTop: '12px' } },
                        React.createElement('button', {
                            type: 'submit',
                            disabled: isSubmitting,
                            style: {
                                padding: '8px 16px',
                                backgroundColor: '#1d7480',
                                color: 'white',
                                border: 'none',
                                borderRadius: '4px',
                                cursor: isSubmitting ? 'not-allowed' : 'pointer'
                            }
                        }, t('add_expense'))
                    )
                ),

                // Expenses list
                expensesLoading ?
                    React.createElement(LoadingSpinner, { message: t('loading_expenses') }) :
                    expenses && expenses.length > 0 ?
                        React.createElement('table', { style: { width: '100%', borderCollapse: 'collapse' } },
                            React.createElement('thead', null,
                                React.createElement('tr', { style: { borderBottom: '2px solid #e0e0e0' } },
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, t('date')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, t('category')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, t('amount')),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, t('actions'))
                                )
                            ),
                            React.createElement('tbody', null,
                                expenses.map(function (expense) {
                                    return React.createElement('tr', { key: expense.id, style: { borderBottom: '1px solid #e0e0e0' } },
                                        React.createElement('td', { style: { padding: '12px' } }, expense.expense_date),
                                        React.createElement('td', { style: { padding: '12px' } }, expense.category),
                                        React.createElement('td', {
                                            style: { padding: '12px', textAlign: 'right', fontWeight: 'bold' }
                                        }, '$' + parseFloat(expense.amount || 0).toFixed(2)),
                                        React.createElement('td', { style: { padding: '12px', textAlign: 'center' } },
                                            React.createElement('button', {
                                                onClick: function () { handleDeleteExpense(expense); },
                                                style: {
                                                    padding: '4px 8px',
                                                    backgroundColor: '#dc2626',
                                                    color: 'white',
                                                    border: 'none',
                                                    borderRadius: '4px',
                                                    cursor: 'pointer',
                                                    fontSize: '12px'
                                                }
                                            }, t('delete'))
                                        )
                                    );
                                })
                            )
                        ) :
                        React.createElement('div', {
                            style: { padding: '20px', textAlign: 'center', color: '#666' }
                        }, t('no_expenses_found'))
            )
        )
    );
}

export default AttendancePage;
