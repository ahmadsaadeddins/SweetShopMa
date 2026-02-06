/**
 * Attendance Tracker Page
 * 
 * Main page for managing employee attendance, payroll, and expenses.
 * Ported from C# SweetShopMa AttendancePage.xaml
 */

import React, { useState, useEffect, useCallback } from 'react';
import { useApi } from '../services/apiService';
import { useAuth } from '../context/AuthContext';
import { useAttendanceRecords, useAttendanceExpenses, useAttendanceSummaries } from '../hooks/useApiData';
import LoadingSpinner from '../components/common/LoadingSpinner';

// Attendance statuses
var ATTENDANCE_STATUSES = [
    { key: 'Present', label: 'Present' },
    { key: 'Reset', label: 'Reset' },
    { key: 'AbsentWithPermission', label: 'Absent With Permission' },
    { key: 'AbsentWithoutPermission', label: 'Absent Without Permission' }
];

// Expense categories
var EXPENSE_CATEGORIES = [
    { key: 'General', label: 'General' },
    { key: 'Supplies', label: 'Supplies' },
    { key: 'Travel', label: 'Travel' },
    { key: 'Meals', label: 'Meals' },
    { key: 'Equipment', label: 'Equipment' },
    { key: 'Other', label: 'Other' }
];

function AttendancePage() {
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
        }
    }, [isReady]);

    async function fetchUsers() {
        try {
            console.log('[AttendancePage] Fetching users...');
            var response = await api.getUsers();
            console.log('[AttendancePage] Users response:', response);

            if (response && Array.isArray(response)) {
                setUsers(response);
            } else if (response && response.results && Array.isArray(response.results)) {
                setUsers(response.results);
            } else {
                console.warn('[AttendancePage] No users found, using fallback');
                setUsers([]);
            }
        } catch (e) {
            console.error('[AttendancePage] Error fetching users:', e);
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
    var setIsErrorStatus = isErrorStatusState[1];

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

    // State for current month
    var currentMonthState = React.useState(
        new Date().toISOString().slice(0, 7)
    );
    var currentMonth = currentMonthState[0];
    var setCurrentMonth = currentMonthState[1];

    // State for sections visibility
    var isAddEditExpandedState = React.useState(true);
    var isAddEditExpanded = isAddEditExpandedState[0];
    var setIsAddEditExpanded = isAddEditExpandedState[1];

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
    var deleteAttendanceRecord = api.deleteAttendanceRecord;
    var getAttendanceSummaries = api.getAttendanceSummaries;
    var getAttendanceExpenses = api.getAttendanceExpenses;
    var createAttendanceExpense = api.createAttendanceExpense;
    var deleteAttendanceExpense = api.deleteAttendanceExpense;

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

    // Handle attendance submission
    var handleSubmitAttendance = async function (e) {
        e.preventDefault();

        if (!selectedUser) {
            showStatus('Please select an employee', true);
            return;
        }

        // Check if record already exists for this user and date
        if (attendanceRecords && attendanceRecords.length > 0) {
            var existingRecord = null;
            for (var i = 0; i < attendanceRecords.length; i++) {
                if (attendanceRecords[i].user === selectedUser.id &&
                    attendanceRecords[i].date === attendanceDate) {
                    existingRecord = attendanceRecords[i];
                    break;
                }
            }

            if (existingRecord) {
                showStatus('Attendance already recorded for ' + (selectedUser.username || selectedUser.email || 'this employee') + ' on ' + attendanceDate, true);
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

            await createAttendanceRecord(data);
            showStatus('Attendance saved successfully', false);
            refetchRecords();
            refetchSummaries();

            setAttendanceNotes('');
        } catch (error) {
            // Check for unique constraint error
            if (error.message && error.message.includes('unique set')) {
                showStatus('Attendance already recorded for this date', true);
            } else {
                showStatus('Error saving attendance: ' + error.message, true);
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    // Handle expense submission
    var handleSubmitExpense = async function (e) {
        e.preventDefault();

        if (!selectedUser) {
            showStatus('Please select an employee', true);
            return;
        }

        if (!expenseAmount || parseFloat(expenseAmount) <= 0) {
            showStatus('Please enter a valid amount', true);
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
            showStatus('Expense added successfully', false);
            refetchExpenses();
            refetchSummaries();

            setExpenseAmount('');
            setExpenseCategory('General');
            setExpenseNotes('');
        } catch (error) {
            showStatus('Error adding expense: ' + error.message, true);
        } finally {
            setIsSubmitting(false);
        }
    };

    // Delete attendance record
    var handleDeleteRecord = async function (record) {
        if (!window.confirm('Are you sure you want to delete this attendance record?')) {
            return;
        }

        try {
            await deleteAttendanceRecord(record.id);
            showStatus('Attendance record deleted', false);
            refetchRecords();
            refetchSummaries();
        } catch (error) {
            showStatus('Error deleting record: ' + error.message, true);
        }
    };

    // Delete expense
    var handleDeleteExpense = async function (expense) {
        if (!window.confirm('Are you sure you want to delete this expense?')) {
            return;
        }

        try {
            await deleteAttendanceExpense(expense.id);
            showStatus('Expense deleted', false);
            refetchExpenses();
            refetchSummaries();
        } catch (error) {
            showStatus('Error deleting expense: ' + error.message, true);
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
                is_present: record ? record.is_present : false
            });
        }

        return days;
    }

    if (!isAuthenticated) {
        return React.createElement('div', null, 'Please log in to access attendance tracker.');
    }

    if (!isReady) {
        return React.createElement(LoadingSpinner, { message: 'Initializing attendance tracker...' });
    }

    if (recordsError) {
        return React.createElement('div', {
            style: { padding: '20px', color: '#dc2626' }
        }, 'Failed to load attendance records: ' + recordsError);
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
                }, title),
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
            }, 'Attendance Tracker'),
            React.createElement('p', {
                style: { color: '#666', marginTop: '5px' }
            }, 'Track employee attendance, payroll, and expenses')
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

        // Add/Edit Attendance Section
        renderSection('Add/Edit Attendance', isAddEditExpanded, setIsAddEditExpanded,
            React.createElement('form', { onSubmit: handleSubmitAttendance },
                React.createElement('div', {
                    style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '12px' }
                },
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, 'Employee'),
                        React.createElement('select', {
                            value: selectedUser ? selectedUser.id : '',
                            onChange: function (e) {
                                var userId = parseInt(e.target.value);
                                var selectedUserObj = users.find(function (u) { return u.id === userId; });
                                setSelectedUser(selectedUserObj || null);
                            },
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        },
                            React.createElement('option', { value: '' }, 'Select Employee'),
                            users.map(function (user) {
                                return React.createElement('option', {
                                    key: user.id,
                                    value: user.id
                                }, user.username || user.email || 'User ' + user.id);
                            })
                        )
                    ),
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, 'Date'),
                        React.createElement('input', {
                            type: 'date',
                            value: attendanceDate,
                            onChange: function (e) { setAttendanceDate(e.target.value); },
                            max: new Date().toISOString().split('T')[0],
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, 'Status'),
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
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, 'Check In'),
                        React.createElement('input', {
                            type: 'time',
                            value: checkInTime,
                            onChange: function (e) { setCheckInTime(e.target.value); },
                            disabled: selectedStatus !== 'Present' && selectedStatus !== 'Reset',
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    React.createElement('div', null,
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, 'Check Out'),
                        React.createElement('input', {
                            type: 'time',
                            value: checkOutTime,
                            onChange: function (e) { setCheckOutTime(e.target.value); },
                            disabled: selectedStatus !== 'Present' && selectedStatus !== 'Reset',
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    ),
                    React.createElement('div', { style: { gridColumn: '1 / -1' } },
                        React.createElement('label', { style: { display: 'block', marginBottom: '4px', fontSize: '14px' } }, 'Notes'),
                        React.createElement('input', {
                            type: 'text',
                            value: attendanceNotes,
                            onChange: function (e) { setAttendanceNotes(e.target.value); },
                            placeholder: 'Optional notes...',
                            style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                        })
                    )
                ),
                React.createElement('div', { style: { marginTop: '16px' } },
                    React.createElement('button', {
                        type: 'submit',
                        disabled: isSubmitting,
                        style: {
                            padding: '10px 20px',
                            backgroundColor: '#1d7480',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: isSubmitting ? 'not-allowed' : 'pointer',
                            opacity: isSubmitting ? 0.7 : 1
                        }
                    }, isSubmitting ? 'Saving...' : 'Record Attendance')
                )
            )
        ),

        // Statistics Section
        stats && renderSection('Statistics', isStatisticsExpanded, setIsStatisticsExpanded,
            React.createElement('div', {
                style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '12px' }
            },
                renderStatCard('Present', stats.presentCount),
                renderStatCard('Absent', stats.absentCount),
                renderStatCard('Overtime', stats.overtimeCount),
                renderStatCard('Regular Hours', stats.totalRegularHours.toFixed(1) + 'h'),
                renderStatCard('OT Hours', stats.totalOvertimeHours.toFixed(1) + 'h'),
                renderStatCard('Payroll', '$' + stats.totalPayroll.toFixed(2))
            )
        ),

        // Calendar Section
        renderSection('Attendance Calendar', isCalendarExpanded, setIsCalendarExpanded,
            React.createElement('div', null,
                // Month selector
                React.createElement('div', { style: { marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '12px' } },
                    React.createElement('button', {
                        onClick: function () {
                            var parts = currentMonth.split('-');
                            var year = parseInt(parts[0]);
                            var month = parseInt(parts[1]);
                            var prevMonth = month === 1 ? 12 : month - 1;
                            var prevYear = month === 1 ? year - 1 : year;
                            setCurrentMonth(prevYear + '-' + prevMonth.toString().padStart(2, '0'));
                        },
                        style: { padding: '8px 12px', cursor: 'pointer' }
                    }, '◀'),
                    React.createElement('input', {
                        type: 'month',
                        value: currentMonth,
                        onChange: function (e) { setCurrentMonth(e.target.value); },
                        style: { padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                    }),
                    React.createElement('button', {
                        onClick: function () {
                            var parts = currentMonth.split('-');
                            var year = parseInt(parts[0]);
                            var month = parseInt(parts[1]);
                            var nextMonth = month === 12 ? 1 : month + 1;
                            var nextYear = month === 12 ? year + 1 : year;
                            setCurrentMonth(nextYear + '-' + nextMonth.toString().padStart(2, '0'));
                        },
                        style: { padding: '8px 12px', cursor: 'pointer' }
                    }, '▶')
                ),
                // Calendar grid
                React.createElement('div', {
                    style: { display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '4px', textAlign: 'center' }
                },
                    ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map(function (day) {
                        return React.createElement('div', {
                            key: day,
                            style: { padding: '8px', fontWeight: 'bold', fontSize: '12px' }
                        }, day);
                    }),
                    calendarDays.map(function (day, index) {
                        return React.createElement('div', {
                            key: index,
                            style: {
                                padding: '12px',
                                minHeight: '60px',
                                backgroundColor: day.isPlaceholder ? 'transparent' : (day.is_present ? '#d4edda' : '#f8d7da'),
                                border: day.isPlaceholder ? 'none' : '1px solid #e0e0e0',
                                borderRadius: '4px',
                                opacity: day.isPlaceholder ? 0.3 : 1
                            }
                        },
                            React.createElement('div', { style: { fontWeight: 'bold' } }, day.day),
                            day.status && React.createElement('div', {
                                style: { fontSize: '10px', marginTop: '4px' }
                            }, day.is_present ? 'P' : 'A')
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
                        React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, 'Employee'),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, 'Present'),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, 'Absent'),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, 'Payroll'),
                        React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, 'Expenses')
                    )
                ),
                React.createElement('tbody', null,
                    monthlySummaries.map(function (summary) {
                        return React.createElement('tr', { key: summary.id, style: { borderBottom: '1px solid #e0e0e0' } },
                            React.createElement('td', { style: { padding: '12px' } }, summary.user_name),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'center' } }, summary.days_present),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'center' } }, summary.days_absent),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'right', fontWeight: 'bold' } },
                                '$' + parseFloat(summary.total_payroll || 0).toFixed(2)),
                            React.createElement('td', { style: { padding: '12px', textAlign: 'right' } },
                                '$' + parseFloat(summary.expenses_total || 0).toFixed(2))
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
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, 'Start Date'),
                            React.createElement('input', {
                                type: 'date',
                                value: filterStartDate,
                                onChange: function (e) { setFilterStartDate(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, 'End Date'),
                            React.createElement('input', {
                                type: 'date',
                                value: filterEndDate,
                                onChange: function (e) { setFilterEndDate(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, 'Search'),
                            React.createElement('input', {
                                type: 'text',
                                value: searchText,
                                onChange: function (e) { setSearchText(e.target.value); },
                                placeholder: 'Search by name...',
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, 'Status'),
                            React.createElement('select', {
                                value: filterStatus,
                                onChange: function (e) { setFilterStatus(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            },
                                React.createElement('option', { value: 'All' }, 'All'),
                                React.createElement('option', { value: 'Present' }, 'Present'),
                                React.createElement('option', { value: 'AbsentWithPermission' }, 'Absent With Permission'),
                                React.createElement('option', { value: 'AbsentWithoutPermission' }, 'Absent Without Permission')
                            )
                        )
                    )
                ),

                // Records table
                recordsLoading ?
                    React.createElement(LoadingSpinner, { message: 'Loading records...' }) :
                    attendanceRecords && attendanceRecords.length > 0 ?
                        React.createElement('table', { style: { width: '100%', borderCollapse: 'collapse' } },
                            React.createElement('thead', null,
                                React.createElement('tr', { style: { borderBottom: '2px solid #e0e0e0' } },
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, 'Date'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, 'Employee'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, 'Status'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, 'Hours'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, 'Pay'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, 'Notes'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, 'Actions')
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
                                            }, 'Delete')
                                        )
                                    );
                                })
                            )
                        ) :
                        React.createElement('div', {
                            style: { padding: '40px', textAlign: 'center', color: '#666' }
                        }, 'No attendance records found for the selected filters.')
            )
        ),

        // Employee Expenses Section
        renderSection('Employee Expenses', isExpensesExpanded, setIsExpensesExpanded,
            React.createElement('div', null,
                // Add Expense Form
                React.createElement('form', { onSubmit: handleSubmitExpense, style: { marginBottom: '20px', padding: '16px', backgroundColor: '#f9f9f9', borderRadius: '4px' } },
                    React.createElement('h3', { style: { fontSize: '14px', fontWeight: 'bold', marginBottom: '12px' } }, 'Add Expense'),
                    React.createElement('div', {
                        style: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '12px' }
                    },
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, 'Date'),
                            React.createElement('input', {
                                type: 'date',
                                value: expenseDate,
                                onChange: function (e) { setExpenseDate(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            })
                        ),
                        React.createElement('div', null,
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, 'Amount'),
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
                            React.createElement('label', { style: { fontSize: '12px', display: 'block', marginBottom: '4px' } }, 'Category'),
                            React.createElement('select', {
                                value: expenseCategory,
                                onChange: function (e) { setExpenseCategory(e.target.value); },
                                style: { width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ddd' }
                            },
                                EXPENSE_CATEGORIES.map(function (c) {
                                    return React.createElement('option', { key: c.key, value: c.key }, c.label);
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
                        }, 'Add Expense')
                    )
                ),

                // Expenses list
                expensesLoading ?
                    React.createElement(LoadingSpinner, { message: 'Loading expenses...' }) :
                    expenses && expenses.length > 0 ?
                        React.createElement('table', { style: { width: '100%', borderCollapse: 'collapse' } },
                            React.createElement('thead', null,
                                React.createElement('tr', { style: { borderBottom: '2px solid #e0e0e0' } },
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, 'Date'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'left' } }, 'Category'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'right' } }, 'Amount'),
                                    React.createElement('th', { style: { padding: '12px', textAlign: 'center' } }, 'Actions')
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
                                            }, 'Delete')
                                        )
                                    );
                                })
                            )
                        ) :
                        React.createElement('div', {
                            style: { padding: '20px', textAlign: 'center', color: '#666' }
                        }, 'No expenses found.')
            )
        )
    );
}

export default AttendancePage;
