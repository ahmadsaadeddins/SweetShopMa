import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert } from '../components/common';
import { useExpenses, useExpenseSummary } from '../hooks';
import { useApi } from '../services';
import { formatCurrency, formatDateTime } from '../utils';
import { useTranslation } from 'react-i18next';

/**
 * ExpensesPage Component
 * Expense tracking with filters and CRUD operations
 */
function ExpensesPage() {
    const { t } = useTranslation();
    const [filters, setFilters] = useState({
        start_date: '',
        end_date: '',
        category: '',
    });

    const [showAddModal, setShowAddModal] = useState(false);
    const [editingExpense, setEditingExpense] = useState(null);
    const [showSummary, setShowSummary] = useState(false);

    const { data: expenses, loading, error, refetch } = useExpenses(filters);
    const { data: summary, loading: summaryLoading } = useExpenseSummary(filters);
    const { deleteExpense } = useApi();

    const columns = [
        { header: t('id'), field: 'id', width: '60px' },
        {
            header: t('date'),
            field: 'date',
            render: (val) => formatDateTime(val),
        },
        { header: t('category'), field: 'category' },
        { header: t('description'), field: 'description' },
        { header: t('amount'), field: 'amount', render: (val) => formatCurrency(val) },
        {
            header: t('actions'),
            field: 'id',
            width: '150px',
            render: (val, row) => (
                <div className="d-flex gap-2">
                    <Button
                        size="small"
                        variant="secondary"
                        onClick={() => handleEdit(row)}
                    >
                        {t('edit')}
                    </Button>
                    <Button
                        size="small"
                        variant="danger"
                        onClick={() => handleDelete(val)}
                    >
                        {t('delete')}
                    </Button>
                </div>
            ),
        },
    ];

    const handleFilterChange = (field, value) => {
        setFilters(prev => ({ ...prev, [field]: value }));
    };

    const handleEdit = (expense) => {
        setEditingExpense(expense);
        setShowAddModal(true);
    };

    const handleDelete = async (id) => {
        if (!window.confirm(t('delete_confirm_expense'))) {
            return;
        }

        try {
            await deleteExpense(id);
            refetch();
        } catch (error) {
            alert(`${t('error_success_deleted')}: ${error.message}`);
        }
    };

    const handleAddNew = () => {
        setEditingExpense(null);
        setShowAddModal(true);
    };

    const totalExpenses = expenses?.reduce((sum, exp) => sum + (exp.amount || 0), 0) || 0;

    return (
        <div className="p-4">
            {/* Header */}
            <div className="d-flex justify-content-between align-items-center mb-4">
                <div>
                    <h1 className="h3 mb-1">{t('expenses')}</h1>
                    <p className="text-muted mb-0">{t('track_manage_expenses')}</p>
                </div>
                <div className="d-flex gap-2">
                    <Button
                        variant="secondary"
                        onClick={() => setShowSummary(!showSummary)}
                    >
                        {showSummary ? t('hide_summary') : t('show_summary')}
                    </Button>
                    <Button variant="primary" onClick={handleAddNew}>
                        {t('add_expense')}
                    </Button>
                </div>
            </div>

            {/* Error Alert */}
            {error && (
                <Alert variant="danger" message={`${t('error_loading_records')}: ${error}`} />
            )}

            {/* Summary Card */}
            {showSummary && (
                <Card className="mb-4" title={t('expense_summary')}>
                    {summaryLoading ? (
                        <div className="text-center py-3">
                            <div className="spinner-border" role="status">
                                <span className="visually-hidden">{t('loading')}</span>
                            </div>
                        </div>
                    ) : summary && summary.length > 0 ? (
                        <div className="row">
                            {summary.map((item, index) => (
                                <div key={index} className="col-12 col-md-6 col-lg-4 mb-3">
                                    <div className="card">
                                        <div className="card-body">
                                            <h6 className="card-subtitle mb-2 text-muted">
                                                {item.category}
                                            </h6>
                                            <h4 className="card-title mb-0">
                                                {formatCurrency(item.total_amount)}
                                            </h4>
                                            <small className="text-muted">
                                                {item.count} {t('expenses')}
                                            </small>
                                        </div>
                                    </div>
                                </div>
                            ))}
                            <div className="col-12">
                                <hr />
                                <div className="d-flex justify-content-between">
                                    <h5>{t('total_expenses')}:</h5>
                                    <h5>{formatCurrency(totalExpenses)}</h5>
                                </div>
                            </div>
                        </div>
                    ) : (
                        <p className="text-muted text-center mb-0">{t('no_summary_data')}</p>
                    )}
                </Card>
            )}

            {/* Filters Card */}
            <Card className="mb-4">
                <div className="row g-3">
                    <div className="col-12 col-md-4">
                        <Input
                            label={t('start_date')}
                            type="date"
                            value={filters.start_date}
                            onChange={(e) => handleFilterChange('start_date', e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-4">
                        <Input
                            label={t('end_date')}
                            type="date"
                            value={filters.end_date}
                            onChange={(e) => handleFilterChange('end_date', e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-4">
                        <Input
                            label={t('category')}
                            placeholder={t('filter_by_category')}
                            value={filters.category}
                            onChange={(e) => handleFilterChange('category', e.target.value)}
                        />
                    </div>
                </div>
            </Card>

            {/* Total Display */}
            <Card className="mb-4">
                <div className="d-flex justify-content-between align-items-center">
                    <span className="h5 mb-0">{t('total_expenses')}:</span>
                    <span className="h3 mb-0 text-danger">{formatCurrency(totalExpenses)}</span>
                </div>
            </Card>

            {/* Expenses Table */}
            <Card>
                <Table
                    columns={columns}
                    data={expenses}
                    loading={loading}
                    emptyMessage={t('no_expenses_found')}
                    keyField="id"
                />
            </Card>

            {/* Add/Edit Modal (Placeholder for now) */}
            {showAddModal && (
                <div className="modal fade show d-block" tabIndex="-1">
                    <div className="modal-dialog">
                        <div className="modal-content">
                            <div className="modal-header">
                                <h5 className="modal-title">
                                    {editingExpense ? t('edit_expense') : t('add_expense')}
                                </h5>
                                <button
                                    type="button"
                                    className="btn-close"
                                    onClick={() => setShowAddModal(false)}
                                ></button>
                            </div>
                            <div className="modal-body">
                                <p>{t('expense_form_placeholder')}</p>
                                <p className="text-muted">
                                    {t('expense_fields_hint')}
                                </p>
                            </div>
                            <div className="modal-footer">
                                <Button
                                    variant="secondary"
                                    onClick={() => setShowAddModal(false)}
                                >
                                    {t('cancel')}
                                </Button>
                                <Button variant="primary">{t('save')}</Button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default ExpensesPage;
