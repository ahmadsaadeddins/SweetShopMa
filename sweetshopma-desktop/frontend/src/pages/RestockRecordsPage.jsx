import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert } from '../components/common';
import { useRestockRecords } from '../hooks';
import { formatCurrency } from '../utils';

/**
 * RestockRecordsPage Component
 * View restock history and audit trail
 */
function RestockRecordsPage() {
    const [filters, setFilters] = useState({
        product_id: '',
        search: '',
    });

    const { data: records, loading, error, refetch } = useRestockRecords(
        filters.product_id || null
    );
    const [searchTerm, setSearchTerm] = useState('');

    // Handle error state - convert error object to readable message
    const errorMessage = error ? (
        typeof error === 'object'
            ? error.message || error.error || JSON.stringify(error)
            : error
    ) : null;

    // Filter records by search term
    const filteredRecords = Array.isArray(records) ? records.filter(record => {
        if (!searchTerm) return true;
        const search = searchTerm.toLowerCase();
        return (
            record.product_name?.toLowerCase().includes(search) ||
            record.user_name?.toLowerCase().includes(search) ||
            record.id?.toString().includes(search)
        );
    }) : [];

    const columns = [
        {
            header: 'Date',
            field: 'restock_date',
            width: '180px',
            render: (val) => {
                if (!val) return '-';
                const date = new Date(val);
                return (
                    <div>
                        <div>{date.toLocaleDateString()}</div>
                        <small className="text-muted">{date.toLocaleTimeString()}</small>
                    </div>
                );
            }
        },
        {
            header: 'Product',
            field: 'product_name',
            render: (val, row) => (
                <div>
                    <div className="fw-bold">{val}</div>
                    {row.product_emoji && (
                        <small className="text-muted">{row.product_emoji}</small>
                    )}
                </div>
            )
        },
        {
            header: 'Quantity Added',
            field: 'quantity_added',
            width: '120px',
            render: (val, row) => (
                <span className="text-success fw-bold">
                    +{val} {row.product_unit || ''}
                </span>
            )
        },
        {
            header: 'Stock Before',
            field: 'stock_before',
            width: '100px',
            render: (val) => val || 0
        },
        {
            header: 'Stock After',
            field: 'stock_after',
            width: '100px',
            render: (val) => (
                <span className="fw-bold">{val}</span>
            )
        },
        {
            header: 'By',
            field: 'user_name',
            width: '120px',
            render: (val) => val || 'Unknown'
        },
        {
            header: 'ID',
            field: 'id',
            width: '60px',
            render: (val) => (
                <small className="text-muted">#{val}</small>
            )
        },
    ];

    const handleFilterChange = (field, value) => {
        setFilters(prev => ({ ...prev, [field]: value }));
    };

    return (
        <div className="p-4">
            {/* Header */}
            <div className="mb-4">
                <h1 className="h3 mb-1">Restock History</h1>
                <p className="text-muted mb-0">Audit trail of inventory restocking operations</p>
            </div>

            {/* Error Alert */}
            {errorMessage && (
                <Alert variant="danger" message={`Error loading restock records: ${errorMessage}`} />
            )}

            {/* Filters Card */}
            <Card className="mb-4">
                <div className="row g-3">
                    <div className="col-12 col-md-6">
                        <Input
                            label="Search"
                            placeholder="Search by product name, user..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-6 d-flex align-items-end">
                        <Button variant="secondary" onClick={() => refetch()}>
                            <i className="bi bi-arrow-clockwise me-2"></i>
                            Refresh
                        </Button>
                    </div>
                </div>
            </Card>

            {/* Stats Cards */}
            <div className="row g-3 mb-4">
                <div className="col-md-4">
                    <Card className="h-100">
                        <div className="text-center">
                            <h3 className="text-primary mb-0">{filteredRecords.length}</h3>
                            <small className="text-muted">Total Records</small>
                        </div>
                    </Card>
                </div>
                <div className="col-md-4">
                    <Card className="h-100">
                        <div className="text-center">
                            <h3 className="text-success mb-0">
                                {filteredRecords.length > 0
                                    ? filteredRecords.reduce((sum, r) => sum + parseFloat(r.quantity_added || 0), 0).toFixed(3)
                                    : '0'}
                            </h3>
                            <small className="text-muted">Total Items Added</small>
                        </div>
                    </Card>
                </div>
                <div className="col-md-4">
                    <Card className="h-100">
                        <div className="text-center">
                            <h3 className="text-warning mb-0">
                                {filteredRecords.length > 0
                                    ? new Set(filteredRecords.map(r => r.user_name).filter(Boolean)).size
                                    : 0}
                            </h3>
                            <small className="text-muted">Unique Staff</small>
                        </div>
                    </Card>
                </div>
            </div>

            {/* Restock Records Table */}
            <Card>
                {loading ? (
                    <div className="text-center py-5">
                        <div className="spinner-border" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                        <p className="mt-2 text-muted">Loading restock records...</p>
                    </div>
                ) : filteredRecords.length === 0 ? (
                    <div className="text-center py-5">
                        <i className="bi bi-inbox text-muted" style={{ fontSize: '3rem' }}></i>
                        <p className="mt-3 text-muted">
                            {searchTerm ? 'No records match your search' : 'No restock records found'}
                        </p>
                        <small className="text-muted">
                            Restock operations will appear here after products are restocked.
                        </small>
                    </div>
                ) : (
                    <Table
                        columns={columns}
                        data={filteredRecords}
                        emptyMessage="No restock records found"
                        keyField="id"
                        striped
                        hover
                    />
                )}
            </Card>

            {/* Info Alert */}
            <Alert
                variant="info"
                message="Only staff users (Admin, Moderator) can restock products. All restock operations are recorded for audit purposes."
                className="mt-4"
            />
        </div>
    );
}

export default RestockRecordsPage;
