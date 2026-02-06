import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert } from '../components/common';
import { useExpenses, useExpenseSummary } from '../hooks';
import { useApi } from '../services';
import { formatCurrency, formatDateTime } from '../utils';

/**
 * ExpensesPage Component
 * Expense tracking with filters and CRUD operations
 */
function ExpensesPage() {
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
        { header: 'ID', field: 'id', width: '60px' },
        {
            header: 'Date',
            field: 'date',
            render: (val) => formatDateTime(val),
        },
        { header: 'Category', field: 'category' },
        { header: 'Description', field: 'description' },
        { header: 'Amount', field: 'amount', render: (val) => formatCurrency(val) },
        {
            header: 'Actions',
            field: 'id',
            width: '150px',
            render: (val, row) => (
                <div className="d-flex gap-2">
                    <Button
                        size="small"
                        variant="secondary"
                        onClick={() => handleEdit(row)}
                    >
                        Edit
                    </Button>
                    <Button
                        size="small"
                        variant="danger"
                        onClick={() => handleDelete(val)}
                    >
                        Delete
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
        if (!window.confirm('Are you sure you want to delete this expense?')) {
            return;
        }

        try {
            await deleteExpense(id);
            refetch();
        } catch (error) {
            alert(`Error deleting expense: ${error.message}`);
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
                    <h1 className="h3 mb-1">Expenses</h1>
                    <p className="text-muted mb-0">Track and manage business expenses</p>
                </div>
                <div className="d-flex gap-2">
                    <Button
                        variant="secondary"
                        onClick={() => setShowSummary(!showSummary)}
                    >
                        {showSummary ? 'Hide' : 'Show'} Summary
                    </Button>
                    <Button variant="primary" onClick={handleAddNew}>
                        Add Expense
                    </Button>
                </div>
            </div>

            {/* Error Alert */}
            {error && (
                <Alert variant="danger" message={`Error loading expenses: ${error}`} />
            )}

            {/* Summary Card */}
            {showSummary && (
                <Card className="mb-4" title="Expense Summary">
                    {summaryLoading ? (
                        <div className="text-center py-3">
                            <div className="spinner-border" role="status">
                                <span className="visually-hidden">Loading...</span>
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
                                                {item.count} expense{item.count !== 1 ? 's' : ''}
                                            </small>
                                        </div>
                                    </div>
                                </div>
                            ))}
                            <div className="col-12">
                                <hr />
                                <div className="d-flex justify-content-between">
                                    <h5>Total Expenses:</h5>
                                    <h5>{formatCurrency(totalExpenses)}</h5>
                                </div>
                            </div>
                        </div>
                    ) : (
                        <p className="text-muted text-center mb-0">No summary data available</p>
                    )}
                </Card>
            )}

            {/* Filters Card */}
            <Card className="mb-4">
                <div className="row g-3">
                    <div className="col-12 col-md-4">
                        <Input
                            label="Start Date"
                            type="date"
                            value={filters.start_date}
                            onChange={(e) => handleFilterChange('start_date', e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-4">
                        <Input
                            label="End Date"
                            type="date"
                            value={filters.end_date}
                            onChange={(e) => handleFilterChange('end_date', e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-4">
                        <Input
                            label="Category"
                            placeholder="Filter by category..."
                            value={filters.category}
                            onChange={(e) => handleFilterChange('category', e.target.value)}
                        />
                    </div>
                </div>
            </Card>

            {/* Total Display */}
            <Card className="mb-4">
                <div className="d-flex justify-content-between align-items-center">
                    <span className="h5 mb-0">Total Expenses:</span>
                    <span className="h3 mb-0 text-danger">{formatCurrency(totalExpenses)}</span>
                </div>
            </Card>

            {/* Expenses Table */}
            <Card>
                <Table
                    columns={columns}
                    data={expenses}
                    loading={loading}
                    emptyMessage="No expenses found"
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
                                    {editingExpense ? 'Edit Expense' : 'Add Expense'}
                                </h5>
                                <button
                                    type="button"
                                    className="btn-close"
                                    onClick={() => setShowAddModal(false)}
                                ></button>
                            </div>
                            <div className="modal-body">
                                <p>Expense form will be implemented here.</p>
                                <p className="text-muted">
                                    This will include fields for date, category, description, amount, etc.
                                </p>
                            </div>
                            <div className="modal-footer">
                                <Button
                                    variant="secondary"
                                    onClick={() => setShowAddModal(false)}
                                >
                                    Cancel
                                </Button>
                                <Button variant="primary">Save</Button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default ExpensesPage;
