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
import { useTranslation } from 'react-i18next';

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

// Helper to translate roles
const getRoleLabel = (role, t) => {
    return t(role.toLowerCase());
};

// Helper to translate activity types
const getActivityLabel = (type, t) => {
    switch (type) {
        case 'login': return t('user_login');
        case 'logout': return t('user_logout');
        case 'create': return t('create_record');
        case 'update': return t('update_record');
        case 'delete': return t('delete_record_activity');
        case 'sale': return t('create_sale_activity');
        case 'restock': return t('restock_product_activity');
        case 'attendance': return t('attendance_action');
        case 'expense': return t('expense_action');
        case 'password_change': return t('password_change');
        case 'role_change': return t('role_change');
        case 'salary_change': return t('salary_change');
        case '': return t('all_activities');
        default: return t('other');
    }
};

function UsersPage() {
    var { t } = useTranslation();
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
            message: t('permissions_error')
        });
    }

    return React.createElement('div', { className: 'page-container' },
        React.createElement('div', { className: 'page-header' },
            React.createElement('h1', null, t('user_management')),
            canManageUsers && React.createElement('button', {
                className: 'btn btn-primary',
                onClick: function () { window.location.href = '#/users?action=add'; }
            }, '+ ' + t('add_user'))
        ),
        React.createElement('div', { className: 'tabs' },
            React.createElement('button', {
                className: 'tab' + (activeTab === 'users' ? ' active' : ''),
                onClick: function () { setActiveTab('users'); }
            }, t('users')),
            canViewReports && React.createElement('button', {
                className: 'tab' + (activeTab === 'activity' ? ' active' : ''),
                onClick: function () { setActiveTab('activity'); }
            }, t('activity_log')),
            canViewReports && React.createElement('button', {
                className: 'tab' + (activeTab === 'salary' ? ' active' : ''),
                onClick: function () { setActiveTab('salary'); }
            }, t('salary_history'))
        ),
        activeTab === 'users' && React.createElement(UsersTab, { canManageUsers: canManageUsers, t: t, getRoleLabel: getRoleLabel }),
        activeTab === 'activity' && canViewReports && React.createElement(ActivityLogTab, { t: t, getActivityLabel: getActivityLabel }),
        activeTab === 'salary' && canViewReports && React.createElement(SalaryHistoryTab, { t: t })
    );
}

function UsersTab(props) {
    var canManageUsers = props.canManageUsers;
    var t = props.t;
    var getRoleLabel = props.getRoleLabel;
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
                alert(t('error_loading_records') + ': ' + (result.error || 'Unknown error'));
            }
        });
    }

    function handleUpdateUser(userId, userData) {
        api.updateUser(userId, userData).then(function (result) {
            if (!result.error) {
                setEditingUser(null);
                refetch();
            } else {
                alert(t('error_loading_records') + ': ' + (result.error || 'Unknown error'));
            }
        });
    }

    function handleDeleteUser(userId) {
        if (confirm(t('delete_confirm_user'))) {
            api.deleteUser(userId).then(function (result) {
                if (!result.error) {
                    setDeletingUser(null);
                    refetch();
                } else {
                    alert(t('error_loading_records') + ': ' + (result.error || 'Unknown error'));
                }
            });
        }
    }

    if (loading) {
        return React.createElement(LoadingSpinner, { message: t('loading_users') });
    }

    if (error) {
        return React.createElement(ErrorMessage, { message: t('error_loading_records') + ': ' + error });
    }

    return React.createElement('div', { className: 'users-tab' },
        React.createElement('div', { className: 'filters-bar' },
            React.createElement('input', {
                type: 'text',
                placeholder: t('search_users'),
                value: searchQuery,
                onChange: function (e) { setSearchQuery(e.target.value); },
                className: 'search-input'
            }),
            React.createElement('select', {
                value: roleFilter,
                onChange: function (e) { setRoleFilter(e.target.value); },
                className: 'role-filter'
            },
                React.createElement('option', { value: '' }, t('all_roles')),
                ROLE_CHOICES.map(function (role) {
                    return React.createElement('option', { key: role.key, value: role.key }, getRoleLabel(role.key, t));
                })
            )
        ),
        React.createElement('table', { className: 'data-table' },
            React.createElement('thead', null,
                React.createElement('tr', null,
                    React.createElement('th', null, t('username')),
                    React.createElement('th', null, t('email')),
                    React.createElement('th', null, t('role')),
                    React.createElement('th', null, t('salary')),
                    React.createElement('th', null, t('status')),
                    React.createElement('th', null, t('actions'))
                )
            ),
            React.createElement('tbody', null,
                filteredUsers.length === 0 ?
                    React.createElement('tr', null,
                        React.createElement('td', { colSpan: 6, style: { textAlign: 'center' } }, t('no_users_found'))
                    ) :
                    filteredUsers.map(function (user) {
                        return React.createElement('tr', { key: user.id },
                            React.createElement('td', null, user.username),
                            React.createElement('td', null, user.email || '-'),
                            React.createElement('td', null,
                                React.createElement('span', { className: 'role-badge role-' + user.role.toLowerCase() }, getRoleLabel(user.role, t))
                            ),
                            React.createElement('td', null, '$' + (parseFloat(user.monthly_salary) || 0).toLocaleString()),
                            React.createElement('td', null,
                                React.createElement('span', {
                                    className: 'status-badge ' + (user.is_active ? 'status-active' : 'status-inactive')
                                }, user.is_active ? t('active') : t('inactive'))
                            ),
                            React.createElement('td', null,
                                React.createElement('div', { className: 'action-buttons' },
                                    canManageUsers && React.createElement('button', {
                                        className: 'btn btn-sm',
                                        onClick: function () { setEditingUser(user); }
                                    }, t('edit')),
                                    React.createElement('button', {
                                        className: 'btn btn-sm btn-secondary',
                                        onClick: function () { setViewingSalaryHistory(user); }
                                    }, t('history')),
                                    canManageUsers && React.createElement('button', {
                                        className: 'btn btn-sm btn-danger',
                                        onClick: function () { setDeletingUser(user); }
                                    }, t('delete'))
                                )
                            )
                        );
                    })
            )
        ),
        showAddModal && React.createElement(UserModal, {
            mode: 'add',
            onSave: handleAddUser,
            onClose: function () { setShowAddModal(null); },
            t: t,
            getRoleLabel: getRoleLabel
        }),
        editingUser && React.createElement(UserModal, {
            mode: 'edit',
            user: editingUser,
            onSave: function (data) { handleUpdateUser(editingUser.id, data); },
            onClose: function () { setEditingUser(null); },
            t: t,
            getRoleLabel: getRoleLabel
        }),
        deletingUser && React.createElement(DeleteConfirmModal, {
            user: deletingUser,
            onConfirm: function () { handleDeleteUser(deletingUser.id); },
            onClose: function () { setDeletingUser(null); },
            t: t
        }),
        viewingSalaryHistory && React.createElement(SalaryHistoryModal, {
            user: viewingSalaryHistory,
            onClose: function () { setViewingSalaryHistory(null); },
            t: t
        })
    );
}

function UserModal(props) {
    var mode = props.mode;
    var user = props.user;
    var onSave = props.onSave;
    var onClose = props.onClose;
    var t = props.t;
    var getRoleLabel = props.getRoleLabel;

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
                alert(t('password_length_error'));
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
                React.createElement('h2', null, mode === 'add' ? t('add_new_user') : t('edit_user')),
                React.createElement('button', { className: 'close-btn', onClick: onClose }, '×')
            ),
            React.createElement('form', { onSubmit: handleSubmit, className: 'modal-body' },
                mode === 'add' && React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, t('username') + ' *'),
                    React.createElement('input', {
                        type: 'text',
                        name: 'username',
                        value: formData.username,
                        onChange: handleChange,
                        required: true
                    })
                ),
                mode === 'add' && React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, t('password') + ' *'),
                    React.createElement('input', {
                        type: 'password',
                        name: 'password',
                        value: formData.password,
                        onChange: handleChange,
                        required: true,
                        minLength: 8,
                        placeholder: t('password_length_error')
                    })
                ),
                React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, t('email')),
                    React.createElement('input', {
                        type: 'email',
                        name: 'email',
                        value: formData.email,
                        onChange: handleChange
                    })
                ),
                React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, t('role')),
                    React.createElement('select', {
                        name: 'role',
                        value: formData.role,
                        onChange: handleChange
                    }, ROLE_CHOICES.map(function (role) {
                        return React.createElement('option', { key: role.key, value: role.key }, getRoleLabel(role.key, t));
                    }))
                ),
                React.createElement('div', { className: 'form-group' },
                    React.createElement('label', null, t('monthly_salary')),
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
                    React.createElement('label', null, t('overtime_multiplier')),
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
                    React.createElement('label', null, t('reason_for_change')),
                    React.createElement('input', {
                        type: 'text',
                        name: 'change_reason',
                        value: formData.change_reason,
                        onChange: handleChange,
                        placeholder: t('optional_document_reason')
                    })
                ),
                React.createElement('div', { className: 'modal-footer' },
                    React.createElement('button', { type: 'button', className: 'btn btn-secondary', onClick: onClose }, t('cancel')),
                    React.createElement('button', { type: 'submit', className: 'btn btn-primary' }, mode === 'add' ? t('create_user_btn') : t('save_changes_btn'))
                )
            )
        )
    );
}

function DeleteConfirmModal(props) {
    var user = props.user;
    var onConfirm = props.onConfirm;
    var onClose = props.onClose;
    var t = props.t;

    return React.createElement('div', { className: 'modal-overlay', onClick: onClose },
        React.createElement('div', { className: 'modal modal-small', onClick: function (e) { e.stopPropagation(); } },
            React.createElement('div', { className: 'modal-header' },
                React.createElement('h2', null, t('delete')),
                React.createElement('button', { className: 'close-btn', onClick: onClose }, '×')
            ),
            React.createElement('div', { className: 'modal-body' },
                React.createElement('p', null, t('delete_confirm_user') + ' "' + user.username + '"?'),
                React.createElement('p', { style: { color: '#e74c3c' } }, t('delete_confirm_product'))
            ),
            React.createElement('div', { className: 'modal-footer' },
                React.createElement('button', { className: 'btn btn-secondary', onClick: onClose }, t('cancel')),
                React.createElement('button', { className: 'btn btn-danger', onClick: onConfirm }, t('delete'))
            )
        )
    );
}

function SalaryHistoryModal(props) {
    var user = props.user;
    var onClose = props.onClose;
    var t = props.t;

    var _useUserSalaryHistory = useUserSalaryHistory(user.id),
        history = _useUserSalaryHistory.data,
        loading = _useUserSalaryHistory.loading,
        error = _useUserSalaryHistory.error;

    return React.createElement('div', { className: 'modal-overlay', onClick: onClose },
        React.createElement('div', { className: 'modal modal-large', onClick: function (e) { e.stopPropagation(); } },
            React.createElement('div', { className: 'modal-header' },
                React.createElement('h2', null, t('salary_history') + ': ' + user.username),
                React.createElement('button', { className: 'close-btn', onClick: onClose }, '×')
            ),
            React.createElement('div', { className: 'modal-body' },
                loading ?
                    React.createElement(LoadingSpinner, { message: t('loading') }) :
                    error ?
                        React.createElement(ErrorMessage, { message: t('error_loading_records') + ': ' + error }) :
                        history && history.length > 0 ?
                            React.createElement('table', { className: 'data-table' },
                                React.createElement('thead', null,
                                    React.createElement('tr', null,
                                        React.createElement('th', null, t('date')),
                                        React.createElement('th', null, t('old_salary')),
                                        React.createElement('th', null, t('new_salary')),
                                        React.createElement('th', null, t('changed_by')),
                                        React.createElement('th', null, t('reason'))
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
                                            React.createElement('td', null, record.changed_by_name || t('unknown')),
                                            React.createElement('td', null, record.change_reason || '-')
                                        );
                                    })
                                )
                            ) :
                            React.createElement('p', { style: { textAlign: 'center', color: '#666' } }, t('no_salary_records_found'))
            ),
            React.createElement('div', { className: 'modal-footer' },
                React.createElement('button', { className: 'btn btn-secondary', onClick: onClose }, t('close'))
            )
        )
    );
}

function ActivityLogTab(props) {
    var t = props.t;
    var getActivityLabel = props.getActivityLabel;
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
        return React.createElement(LoadingSpinner, { message: t('loading_activities') });
    }

    if (error) {
        return React.createElement(ErrorMessage, { message: t('error_loading_records') + ': ' + error });
    }

    return React.createElement('div', { className: 'activity-tab' },
        React.createElement('div', { className: 'filters-bar' },
            React.createElement('select', {
                value: queryFilters.activity_type,
                onChange: function (e) { handleFilterChange('activity_type', e.target.value); }
            }, ACTIVITY_TYPES.map(function (type) {
                return React.createElement('option', { key: type.key, value: type.key }, getActivityLabel(type.key, t));
            })),
            React.createElement('input', {
                type: 'date',
                placeholder: t('start_date'),
                value: queryFilters.start_date || '',
                onChange: function (e) { handleFilterChange('start_date', e.target.value); }
            }),
            React.createElement('input', {
                type: 'date',
                placeholder: t('end_date'),
                value: queryFilters.end_date || '',
                onChange: function (e) { handleFilterChange('end_date', e.target.value); }
            })
        ),
        React.createElement('table', { className: 'data-table' },
            React.createElement('thead', null,
                React.createElement('tr', null,
                    React.createElement('th', null, t('timestamp')),
                    React.createElement('th', null, t('user')),
                    React.createElement('th', null, t('activity_log')),
                    React.createElement('th', null, t('description')),
                    React.createElement('th', null, t('resource'))
                )
            ),
            React.createElement('tbody', null,
                activities && activities.length > 0 ?
                    activities.map(function (activity) {
                        return React.createElement('tr', { key: activity.id },
                            React.createElement('td', null, activity.timestamp_display),
                            React.createElement('td', null, activity.user_name || t('unknown')),
                            React.createElement('td', null,
                                React.createElement('span', { className: 'activity-badge activity-' + activity.activity_type },
                                    getActivityLabel(activity.activity_type, t)
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
                        React.createElement('td', { colSpan: 5, style: { textAlign: 'center' } }, t('no_activities_found'))
                    )
            )
        )
    );
}

function SalaryHistoryTab(props) {
    var t = props.t;
    var _useAllSalaryHistory = useAllSalaryHistory(),
        history = _useAllSalaryHistory.data,
        loading = _useAllSalaryHistory.loading,
        error = _useAllSalaryHistory.error;

    if (loading) {
        return React.createElement(LoadingSpinner, { message: t('loading') });
    }

    if (error) {
        return React.createElement(ErrorMessage, { message: t('error_loading_records') + ': ' + error });
    }

    return React.createElement('div', { className: 'salary-history-tab' },
        React.createElement('table', { className: 'data-table' },
            React.createElement('thead', null,
                React.createElement('tr', null,
                    React.createElement('th', null, t('date')),
                    React.createElement('th', null, t('user')),
                    React.createElement('th', null, t('old_salary')),
                    React.createElement('th', null, t('new_salary')),
                    React.createElement('th', null, t('changed_by')),
                    React.createElement('th', null, t('reason'))
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
                                        t('user') + ' #' + record.user_profile :
                                    t('unknown')
                            ),
                            React.createElement('td', null, '$' + parseFloat(record.old_salary).toLocaleString()),
                            React.createElement('td', null,
                                React.createElement('strong', null, '$' + parseFloat(record.new_salary).toLocaleString())
                            ),
                            React.createElement('td', null, record.changed_by_name || t('unknown')),
                            React.createElement('td', null, record.change_reason || '-')
                        );
                    }) :
                    React.createElement('tr', null,
                        React.createElement('td', { colSpan: 6, style: { textAlign: 'center' } }, t('no_salary_records_found'))
                    )
            )
        )
    );
}

export default UsersPage;
