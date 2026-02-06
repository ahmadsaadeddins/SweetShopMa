/**
 * Users Management Page
 * 
 * Comprehensive user management with:
 * - User list with CRUD operations
 * - Salary history tracking
 * - Activity logging
 */

import React, { useState, useEffect, useCallback } from 'react';
import { useApi } from '../services/apiService';
import { useAuth } from '../context/AuthContext';
import { useUsers, useUserSalaryHistory, useAllSalaryHistory, useAllActivities } from '../hooks/useApiData';
import LoadingSpinner from '../components/common/LoadingSpinner';
import ErrorMessage from '../components/common/ErrorMessage';

// Role choices for dropdown
var ROLE_CHOICES = [
    { key: 'Developer', label: 'Developer' },
    { key: 'Admin', label: 'Admin' },
    { key: 'Moderator', label: 'Moderator' },
    { key: 'Employee', label: 'Employee' },
    { key: 'Seller', label: 'Seller' }
];

// Activity type choices for filter
var ACTIVITY_TYPES = [
    { key: '', label: 'All Activities' },
    { key: 'login', label: 'User Login' },
    { key: 'logout', label: 'User Logout' },
    { key: 'create', label: 'Create Record' },
    { key: 'update', label: 'Update Record' },
    { key: 'delete', label: 'Delete Record' },
    { key: 'sale', label: 'Create Sale' },
    { key: 'restock', label: 'Restock Product' },
    { key: 'attendance', label: 'Attendance Action' },
    { key: 'expense', label: 'Expense Action' },
    { key: 'password_change', label: 'Password Change' },
    { key: 'role_change', label: 'Role Change' },
    { key: 'salary_change', label: 'Salary Change' },
    { key: 'other', label: 'Other' }
];

function UsersPage() {
    var api = useApi();
    var auth = useAuth();
    var isReady = api.isReady;

    // Tab state
    var [activeTab, setActiveTab] = useState('users');

    // Check permissions
    var canManageUsers = auth.user?.role === 'Developer' || auth.user?.role === 'Admin';
    var canViewReports = auth.user?.role === 'Developer' || auth.user?.role === 'Admin' || auth.user?.role === 'Moderator';

    if (!canManageUsers && !canViewReports) {
        return React.createElement(ErrorMessage, {
            message: 'You do not have permission to access this page.'
        });
    }

    return React.createElement('div', { className: 'page-container' },
        React.createElement('div', { className: 'page-header' },
            React.createElement('h1', null, 'User Management'),
            canManageUsers && React.createElement('button', {
                className: 'btn btn-primary',
                onClick: function () { window.location.href = '#/users?action=add'; }
            }, '+ Add User')
        ),
        React.createElement('div', { className: 'tabs' },
            React.createElement('button', {
                className: 'tab' + (activeTab === 'users' ? ' active' : ''),
                onClick: function () { setActiveTab('users'); }
            }, 'Users'),
            canViewReports && React.createElement('button', {
                className: 'tab' + (activeTab === 'activity' ? ' active' : ''),
                onClick: function () { setActiveTab('activity'); }
            }, 'Activity Log'),
            canViewReports && React.createElement('button', {
                className: 'tab' + (activeTab === 'salary' ? ' active' : ''),
                onClick: function () { setActiveTab('salary'); }
            }, 'Salary History')
        ),
        activeTab === 'users' && React.createElement(UsersTab, { canManageUsers: canManageUsers }),
        activeTab === 'activity' && canViewReports && React.createElement(ActivityLogTab, null),
        activeTab === 'salary' && canViewReports && React.createElement(SalaryHistoryTab, null)
    );
}

function UsersTab(props) {
    var canManageUsers = props.canManageUsers;
    var api = useApi();
    var isReady = api.isReady;

    var _useUsers = useUsers(),
        users = _useUsers.data,
        loading = _useUsers.loading,
        error = _useUsers.error,
        refetch = _useUsers.refetch;

    var _useState = useState(''),
        searchQuery = _useState[0],
        setSearchQuery = _useState[1];

    var _useState2 = useState(''),
        roleFilter = _useState2[0],
        setRoleFilter = _useState2[1];

    var _useState3 = useState(null),
        showAddModal = _useState3[0],
        setShowAddModal = _useState3[1];

    var _useState4 = useState(null),
        editingUser = _useState4[0],
        setEditingUser = _useState4[1];

    var _useState5 = useState(null),
        deletingUser = _useState5[0],
        setDeletingUser = _useState5[1];

    var _useState6 = useState(null),
        viewingSalaryHistory = _useState6[0],
        setViewingSalaryHistory = _useState6[1];

    var filteredUsers = users ? users.filter(function (user) {
        var matchesSearch = user.username.toLowerCase().indexOf(searchQuery.toLowerCase()) !== -1 ||
            (user.email && user.email.toLowerCase().indexOf(searchQuery.toLowerCase()) !== -1);
        var matchesRole = !roleFilter || user.role === roleFilter;
        return matchesSearch && matchesRole;
    }) : [];

    function handleAddUser(userData) {
        api.createUser(userData).then(function (result) {
            if (!result.error) {
                setShowAddModal(null);
                refetch();
            } else {
                alert('Error creating user: ' + (result.error || 'Unknown error'));
            }
        });
    }

    function handleUpdateUser(userId, userData) {
        api.updateUser(userId, userData).then(function (result) {
            if (!result.error) {
                setEditingUser(null);
                refetch();
            } else {
                alert('Error updating user: ' + (result.error || 'Unknown error'));
            }
        });
    }

    function handleDeleteUser(userId) {
        if (confirm('Are you sure you want to delete this user?')) {
            api.deleteUser(userId).then(function (result) {
                if (!result.error) {
                    setDeletingUser(null);
                    refetch();
                } else {
                    alert('Error deleting user: ' + (result.error || 'Unknown error'));
                }
            });
        }
    }

    if (loading) {
        return React.createElement(LoadingSpinner, { message: 'Loading users...' });
    }

    if (error) {
        return React.createElement(ErrorMessage, { message: 'Error loading users: ' + error });
    }

    return React.createElement('div', { className: 'users-tab' },
        React.createElement('div', { className: 'filters-bar' },
            React.createElement('input', {
                type: 'text',
                placeholder: 'Search users...',
                value: searchQuery,
                onChange: function (e) { setSearchQuery(e.target.value); },
                className: 'search-input'
            }),
            React.createElement('select', {
                value: roleFilter,
                onChange: function (e) { setRoleFilter(e.target.value); },
                className: 'role-filter'
            },
                React.createElement('option', { value: '' }, 'All Roles'),
                ROLE_CHOICES.map(function (role) {
                    return React.createElement('option', { key: role.key, value: role.key }, role.label);
                })
            )
        ),
        React.createElement('table', { className: 'data-table' },
            React.createElement('thead', null,
                React.createElement('tr', null,
                    React.createElement('th', null, 'Username'),
                    React.createElement('th', null, 'Email'),
                    React.createElement('th', null, 'Role'),
                    React.createElement('th', null, 'Salary'),
                    React.createElement('th', null, 'Status'),
                    React.createElement('th', null, 'Actions')
                )
            ),
            React.createElement('tbody', null,
                filteredUsers.length === 0 ?
                    React.createElement('tr', null,
                        React.createElement('td', { colSpan: 6, style: { textAlign: 'center' } }, 'No users found')
                    ) :
                    filteredUsers.map(function (user) {
                        return React.createElement('tr', { key: user.id },
                            React.createElement('td', null, user.username),
                            React.createElement('td', null, user.email || '-'),
                            React.createElement('td', null,
                                React.createElement('span', { className: 'role-badge role-' + user.role.toLowerCase() }, user.role)
                            ),
                            React.createElement('td', null, '$' + (parseFloat(user.monthly_salary) || 0).toLocaleString()),
                            React.createElement('td', null,
                                React.createElement('span', {
                                    className: 'status-badge ' + (user.is_active ? 'status-active' : 'status-inactive')
                                }, user.is_active ? 'Active' : 'Inactive')
                            ),
                            React.createElement('td', null,
                                React.createElement('div', { className: 'action-buttons' },
                                    canManageUsers && React.createElement('button', {
                                        className: 'btn btn-sm',
                                        onClick: function () { setEditingUser(user); }
                                    }, 'Edit'),
                                    React.createElement('button', {
                                        className: 'btn btn-sm btn-secondary',
                                        onClick: function () { setViewingSalaryHistory(user); }
                                    }, 'History'),
                                    canManageUsers && React.createElement('button', {
                                        className: 'btn btn-sm btn-danger',
                                        onClick: function () { setDeletingUser(user); }
                                    }, 'Delete')
                                )
                            )
                        );
                    })
            )
        ),
        showAddModal && React.createElement(UserModal, {
            mode: 'add',
            onSave: handleAddUser,
            onClose: function () { setShowAddModal(null); }
        }),
        editingUser && React.createElement(UserModal, {
            mode: 'edit',
            user: editingUser,
            onSave: function (data) { handleUpdateUser(editingUser.id, data); },
            onClose: function () { setEditingUser(null); }
        }),
        deletingUser && React.createElement(DeleteConfirmModal, {
            user: deletingUser,
            onConfirm: function () { handleDeleteUser(deletingUser.id); },
            onClose: function () { setDeletingUser(null); }
        }),
        viewingSalaryHistory && React.createElement(SalaryHistoryModal, {
            user: viewingSalaryHistory,
            onClose: function () { setViewingSalaryHistory(null); }
        })
    );
}

function UserModal(props) {
    var mode = props.mode;
    var user = props.user;
    var onSave = props.onSave;
    var onClose = props.onClose;

    var _useState7 = useState({
        username: '',
        password: '',
        email: '',
        role: 'Seller',
        monthly_salary: 5000,
        overtime_multiplier: 1.5,
        change_reason: ''
    }),
        formData = _useState7[0],
        setFormData = _useState7[1];

    useEffect(function () {
        if (mode === 'edit' && user) {
            setFormData({
                username: user.username,
                email: user.email || '',
                role: user.role,
                monthly_salary: user.monthly_salary,
                overtime_multiplier: user.overtime_multiplier,
                change_reason: ''
            });
        }
    }, [mode, user]);

    function handleChange(e) {
        var name = e.target.name;
        var value = e.target.value;
        setFormData(function (prev) {
            var newData = Object.assign({}, prev);
            newData[name] = value;
            return newData;
        });
    }

    function handleSubmit(e) {
        e.preventDefault();
        var data = Object.assign({}, formData);
        if (mode === 'add') {
            if (!data.password || data.password.length < 8) {
                alert('Password must be at least 8 characters');
                return;
            }
        } else {
            delete data.username;
            delete data.password;
        }
        data.monthly_salary = parseFloat(data.monthly_salary) || 0;
        data.overtime_multiplier = parseFloat(data.overtime_multiplier) || 1.5;
        onSave(data);
    }

    return React.createElement('div', { className: 'modal-overlay', onClick: onClose },
        React.createElement('div', { className: 'modal', onClick: function (e) { e.stopPropagation(); } },
            React.createElement('div', { className: 'modal-header' },
                React.createElement('h2', null, mode === 'add' ? 'Add New User' : 'Edit User'),
                React.createElement('button', { className: 'close-btn', onClick: onClose }, '×')
            ),
            React.createElement('form', { onSubmit: handleSubmit, className: 'modal-body' },
                mode === 'add' && React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, 'Username *'),
                    React.createElement('input', {
                        type: 'text',
                        name: 'username',
                        value: formData.username,
                        onChange: handleChange,
                        required: true
                    })
                ),
                mode === 'add' && React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, 'Password *'),
                    React.createElement('input', {
                        type: 'password',
                        name: 'password',
                        value: formData.password,
                        onChange: handleChange,
                        required: true,
                        minLength: 8,
                        placeholder: 'At least 8 characters'
                    })
                ),
                React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, 'Email'),
                    React.createElement('input', {
                        type: 'email',
                        name: 'email',
                        value: formData.email,
                        onChange: handleChange
                    })
                ),
                React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, 'Role'),
                    React.createElement('select', {
                        name: 'role',
                        value: formData.role,
                        onChange: handleChange
                    }, ROLE_CHOICES.map(function (role) {
                        return React.createElement('option', { key: role.key, value: role.key }, role.label);
                    }))
                ),
                React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, 'Monthly Salary'),
                    React.createElement('input', {
                        type: 'number',
                        name: 'monthly_salary',
                        value: formData.monthly_salary,
                        onChange: handleChange,
                        min: 0,
                        step: 100
                    })
                ),
                React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, 'Overtime Multiplier'),
                    React.createElement('input', {
                        type: 'number',
                        name: 'overtime_multiplier',
                        value: formData.overtime_multiplier,
                        onChange: handleChange,
                        min: 1,
                        max: 5,
                        step: 0.1
                    })
                ),
                mode === 'edit' && React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, 'Reason for Change'),
                    React.createElement('input', {
                        type: 'text',
                        name: 'change_reason',
                        value: formData.change_reason,
                        onChange: handleChange,
                        placeholder: 'Optional: Document why you are making this change'
                    })
                ),
                React.createElement('div', { className: 'modal-footer' },
                    React.createElement('button', { type: 'button', className: 'btn btn-secondary', onClick: onClose }, 'Cancel'),
                    React.createElement('button', { type: 'submit', className: 'btn btn-primary' }, mode === 'add' ? 'Create User' : 'Save Changes')
                )
            )
        )
    );
}

function DeleteConfirmModal(props) {
    var user = props.user;
    var onConfirm = props.onConfirm;
    var onClose = props.onClose;

    return React.createElement('div', { className: 'modal-overlay', onClick: onClose },
        React.createElement('div', { className: 'modal modal-small', onClick: function (e) { e.stopPropagation(); } },
            React.createElement('div', { className: 'modal-header' },
                React.createElement('h2', null, 'Delete User'),
                React.createElement('button', { className: 'close-btn', onClick: onClose }, '×')
            ),
            React.createElement('div', { className: 'modal-body' },
                React.createElement('p', null, 'Are you sure you want to delete user "' + user.username + '"?'),
                React.createElement('p', { style: { color: '#e74c3c' } }, 'This action cannot be undone.')
            ),
            React.createElement('div', { className: 'modal-footer' },
                React.createElement('button', { className: 'btn btn-secondary', onClick: onClose }, 'Cancel'),
                React.createElement('button', { className: 'btn btn-danger', onClick: onConfirm }, 'Delete')
            )
        )
    );
}

function SalaryHistoryModal(props) {
    var user = props.user;
    var onClose = props.onClose;

    var _useUserSalaryHistory = useUserSalaryHistory(user.id),
        history = _useUserSalaryHistory.data,
        loading = _useUserSalaryHistory.loading,
        error = _useUserSalaryHistory.error;

    return React.createElement('div', { className: 'modal-overlay', onClick: onClose },
        React.createElement('div', { className: 'modal modal-large', onClick: function (e) { e.stopPropagation(); } },
            React.createElement('div', { className: 'modal-header' },
                React.createElement('h2', null, 'Salary History: ' + user.username),
                React.createElement('button', { className: 'close-btn', onClick: onClose }, '×')
            ),
            React.createElement('div', { className: 'modal-body' },
                loading ?
                    React.createElement(LoadingSpinner, { message: 'Loading salary history...' }) :
                    error ?
                        React.createElement(ErrorMessage, { message: 'Error loading history: ' + error }) :
                        history && history.length > 0 ?
                            React.createElement('table', { className: 'data-table' },
                                React.createElement('thead', null,
                                    React.createElement('tr', null,
                                        React.createElement('th', null, 'Date'),
                                        React.createElement('th', null, 'Old Salary'),
                                        React.createElement('th', null, 'New Salary'),
                                        React.createElement('th', null, 'Changed By'),
                                        React.createElement('th', null, 'Reason')
                                    )
                                ),
                                React.createElement('tbody', null,
                                    history.map(function (record) {
                                        return React.createElement('tr', { key: record.id },
                                            React.createElement('td', null, new Date(record.changed_at).toLocaleString()),
                                            React.createElement('td', null, '$' + parseFloat(record.old_salary).toLocaleString()),
                                            React.createElement('td', null,
                                                React.createElement('strong', null, '$' + parseFloat(record.new_salary).toLocaleString())
                                            ),
                                            React.createElement('td', null, record.changed_by_name || 'Unknown'),
                                            React.createElement('td', null, record.change_reason || '-')
                                        );
                                    })
                                )
                            ) :
                            React.createElement('p', { style: { textAlign: 'center', color: '#666' } }, 'No salary history records found.')
            ),
            React.createElement('div', { className: 'modal-footer' },
                React.createElement('button', { className: 'btn btn-secondary', onClick: onClose }, 'Close')
            )
        )
    );
}

function ActivityLogTab() {
    var _useState8 = useState({}),
        filters = _useState8[0],
        setFilters = _useState8[1];

    var _useState9 = useState({ user_id: '', activity_type: '', limit: 100 }),
        queryFilters = _useState9[0],
        setQueryFilters = _useState9[1];

    var _useAllActivities = useAllActivities(queryFilters),
        activities = _useAllActivities.data,
        loading = _useAllActivities.loading,
        error = _useAllActivities.error;

    function handleFilterChange(name, value) {
        setQueryFilters(function (prev) {
            var newFilters = Object.assign({}, prev);
            newFilters[name] = value;
            return newFilters;
        });
    }

    if (loading) {
        return React.createElement(LoadingSpinner, { message: 'Loading activity log...' });
    }

    if (error) {
        return React.createElement(ErrorMessage, { message: 'Error loading activities: ' + error });
    }

    return React.createElement('div', { className: 'activity-tab' },
        React.createElement('div', { className: 'filters-bar' },
            React.createElement('select', {
                value: queryFilters.activity_type,
                onChange: function (e) { handleFilterChange('activity_type', e.target.value); }
            }, ACTIVITY_TYPES.map(function (type) {
                return React.createElement('option', { key: type.key, value: type.key }, type.label);
            })),
            React.createElement('input', {
                type: 'date',
                placeholder: 'Start Date',
                value: queryFilters.start_date || '',
                onChange: function (e) { handleFilterChange('start_date', e.target.value); }
            }),
            React.createElement('input', {
                type: 'date',
                placeholder: 'End Date',
                value: queryFilters.end_date || '',
                onChange: function (e) { handleFilterChange('end_date', e.target.value); }
            })
        ),
        React.createElement('table', { className: 'data-table' },
            React.createElement('thead', null,
                React.createElement('tr', null,
                    React.createElement('th', null, 'Timestamp'),
                    React.createElement('th', null, 'User'),
                    React.createElement('th', null, 'Activity'),
                    React.createElement('th', null, 'Description'),
                    React.createElement('th', null, 'Resource')
                )
            ),
            React.createElement('tbody', null,
                activities && activities.length > 0 ?
                    activities.map(function (activity) {
                        return React.createElement('tr', { key: activity.id },
                            React.createElement('td', null, activity.timestamp_display),
                            React.createElement('td', null, activity.user_name || 'Unknown'),
                            React.createElement('td', null,
                                React.createElement('span', { className: 'activity-badge activity-' + activity.activity_type },
                                    ACTIVITY_TYPES.find(function (t) { return t.key === activity.activity_type; })?.label || activity.activity_type
                                )
                            ),
                            React.createElement('td', null, activity.description || '-'),
                            React.createElement('td', null,
                                activity.resource_type ?
                                    '#' + activity.resource_id + ' (' + activity.resource_type + ')' :
                                    '-'
                            )
                        );
                    }) :
                    React.createElement('tr', null,
                        React.createElement('td', { colSpan: 5, style: { textAlign: 'center' } }, 'No activities found')
                    )
            )
        )
    );
}

function SalaryHistoryTab() {
    var _useAllSalaryHistory = useAllSalaryHistory(),
        history = _useAllSalaryHistory.data,
        loading = _useAllSalaryHistory.loading,
        error = _useAllSalaryHistory.error;

    if (loading) {
        return React.createElement(LoadingSpinner, { message: 'Loading salary history...' });
    }

    if (error) {
        return React.createElement(ErrorMessage, { message: 'Error loading salary history: ' + error });
    }

    return React.createElement('div', { className: 'salary-history-tab' },
        React.createElement('table', { className: 'data-table' },
            React.createElement('thead', null,
                React.createElement('tr', null,
                    React.createElement('th', null, 'Date'),
                    React.createElement('th', null, 'User'),
                    React.createElement('th', null, 'Old Salary'),
                    React.createElement('th', null, 'New Salary'),
                    React.createElement('th', null, 'Changed By'),
                    React.createElement('th', null, 'Reason')
                )
            ),
            React.createElement('tbody', null,
                history && history.length > 0 ?
                    history.map(function (record) {
                        return React.createElement('tr', { key: record.id },
                            React.createElement('td', null, new Date(record.changed_at).toLocaleString()),
                            React.createElement('td', null,
                                record.user_profile ?
                                    record.user_profile.user ?
                                        record.user_profile.user.username :
                                        'User #' + record.user_profile :
                                    'Unknown'
                            ),
                            React.createElement('td', null, '$' + parseFloat(record.old_salary).toLocaleString()),
                            React.createElement('td', null,
                                React.createElement('strong', null, '$' + parseFloat(record.new_salary).toLocaleString())
                            ),
                            React.createElement('td', null, record.changed_by_name || 'Unknown'),
                            React.createElement('td', null, record.change_reason || '-')
                        );
                    }) :
                    React.createElement('tr', null,
                        React.createElement('td', { colSpan: 6, style: { textAlign: 'center' } }, 'No salary history records found.')
                    )
            )
        )
    );
}

export default UsersPage;
