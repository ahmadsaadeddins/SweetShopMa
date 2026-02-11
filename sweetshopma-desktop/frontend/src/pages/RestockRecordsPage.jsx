import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert } from '../components/common';
import { useRestockRecords } from '../hooks';
import { formatCurrency } from '../utils';
import { useTranslation } from 'react-i18next';

/**
 * RestockRecordsPage Component
 * View restock history and audit trail
 */
function RestockRecordsPage() {
    const { t } = useTranslation();
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
            header: t('date'),
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
            header: t('product'),
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
            header: t('quantity_added'),
            field: 'quantity_added',
            width: '120px',
            render: (val, row) => (
                <span className="text-success fw-bold">
                    +{val} {row.product_unit || ''}
                </span>
            )
        },
        {
            header: t('stock_before'),
            field: 'stock_before',
            width: '100px',
            render: (val) => val || 0
        },
        {
            header: t('stock_after'),
            field: 'stock_after',
            width: '100px',
            render: (val) => (
                <span className="fw-bold">{val}</span>
            )
        },
        {
            header: t('by'),
            field: 'user_name',
            width: '120px',
            render: (val) => val || t('unknown')
        },
        {
            header: t('id'),
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
                <h1 className="h3 mb-1">{t('restock_history')}</h1>
                <p className="text-muted mb-0">{t('restock_audit_trail')}</p>
            </div>

            {/* Error Alert */}
            {errorMessage && (
                <Alert variant="danger" message={t('error_loading_restock_records', { error: errorMessage })} />
            )}

            {/* Filters Card */}
            <Card className="mb-4">
                <div className="row g-3">
                    <div className="col-12 col-md-6">
                        <Input
                            label={t('search')}
                            placeholder={t('search_products')}
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-6 d-flex align-items-end">
                        <Button variant="secondary" onClick={() => refetch()}>
                            <i className="bi bi-arrow-clockwise me-2"></i>
                            {t('refresh')}
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
                            <small className="text-muted">{t('total_records')}</small>
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
                            <small className="text-muted">{t('total_items_added')}</small>
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
                            <small className="text-muted">{t('unique_staff')}</small>
                        </div>
                    </Card>
                </div>
            </div>

            {/* Restock Records Table */}
            <Card>
                {loading ? (
                    <div className="text-center py-5">
                        <div className="spinner-border" role="status">
                            <span className="visually-hidden">{t('loading')}</span>
                        </div>
                        <p className="mt-2 text-muted">{t('loading_restock_records')}</p>
                    </div>
                ) : filteredRecords.length === 0 ? (
                    <div className="text-center py-5">
                        <i className="bi bi-inbox text-muted" style={{ fontSize: '3rem' }}></i>
                        <p className="mt-3 text-muted">
                            {searchTerm ? t('no_records_match_search') : t('no_records_found')}
                        </p>
                        <small className="text-muted">
                            {t('restock_empty_hint')}
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
                message={t('restock_permission_info')}
                className="mt-4"
            />
        </div>
    );
}

export default RestockRecordsPage;
