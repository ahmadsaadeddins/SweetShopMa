import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert } from '../components/common';
import { useSales, useSale } from '../hooks';
import { useApi } from '../services';
import { formatCurrency, formatDateTime } from '../utils';
import { useTranslation } from 'react-i18next';

/**
 * SalesPage Component
 * Sales history with filters and details
 */
function SalesPage() {
    const { t } = useTranslation();
    const [filters, setFilters] = useState({
        start_date: '',
        end_date: '',
        status: '',
    });

    const [selectedSale, setSelectedSale] = useState(null);
    const [showDetailsModal, setShowDetailsModal] = useState(false);

    const { data: sales, loading, error, refetch } = useSales(filters);
    const { data: saleDetails, loading: detailsLoading } = useSale(selectedSale?.id);
    const { refundSale } = useApi();

    const columns = [
        { header: t('id'), field: 'id', width: '80px' },
        {
            header: t('date'),
            field: 'created_at',
            render: (val) => formatDateTime(val),
        },
        { header: t('items_table_header'), field: 'item_count', width: '80px', render: (val) => val || 0 },
        { header: t('subtotal'), field: 'subtotal', render: (val) => formatCurrency(val) },
        { header: t('tax'), field: 'tax', render: (val) => formatCurrency(val) },
        { header: t('discount_amount'), field: 'discount', render: (val) => formatCurrency(val) },
        { header: t('total'), field: 'total', render: (val) => formatCurrency(val) },
        {
            header: t('payment'),
            field: 'payment_method',
            width: '100px',
            render: (val) => val ? t(val.toLowerCase()) : '-',
        },
        {
            header: t('status'),
            field: 'status',
            width: '100px',
            render: (val) => (
                <span className={`badge bg-${val === 'completed' ? 'success' : val === 'refunded' ? 'danger' : 'warning'}`}>
                    {val ? t(val.toLowerCase()) : '-'}
                </span>
            ),
        },
        {
            header: t('actions'),
            field: 'id',
            width: '120px',
            render: (val, row) => (
                <div className="d-flex gap-2">
                    <Button
                        size="small"
                        variant="secondary"
                        onClick={() => handleViewDetails(row)}
                    >
                        {t('view')}
                    </Button>
                    {row.status === 'completed' && (
                        <Button
                            size="small"
                            variant="warning"
                            onClick={() => handleRefund(val)}
                        >
                            {t('refund')}
                        </Button>
                    )}
                </div>
            ),
        },
    ];

    const handleFilterChange = (field, value) => {
        setFilters(prev => ({ ...prev, [field]: value }));
    };

    const handleViewDetails = (sale) => {
        setSelectedSale(sale);
        setShowDetailsModal(true);
    };

    const handleRefund = async (id) => {
        const reason = window.prompt(t('refund_reason_prompt'));
        if (!reason) return;

        try {
            await refundSale(id, reason);
            refetch();
            alert(t('refund_success'));
        } catch (error) {
            alert(`${t('error_refunding_sale')}: ${error.message}`);
        }
    };

    const getStatusBadgeClass = (status) => {
        switch (status) {
            case 'completed':
                return 'bg-success';
            case 'refunded':
                return 'bg-danger';
            case 'cancelled':
                return 'bg-secondary';
            default:
                return 'bg-warning';
        }
    };

    return (
        <div className="p-4">
            {/* Header */}
            <div className="mb-4">
                <h1 className="h3 mb-1">{t('sales')}</h1>
                <p className="text-muted mb-0">{t('view_manage_sales')}</p>
            </div>

            {/* Error Alert */}
            {error && (
                <Alert variant="danger" message={`${t('error_loading_records')}: ${error}`} />
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
                        <Select
                            label={t('status')}
                            value={filters.status}
                            onChange={(e) => handleFilterChange('status', e.target.value)}
                            options={[
                                { value: '', label: t('all_statuses') },
                                { value: 'completed', label: t('completed') },
                                { value: 'refunded', label: t('refunded') },
                                { value: 'cancelled', label: t('cancelled') },
                            ]}
                        />
                    </div>
                </div>
            </Card>

            {/* Sales Table */}
            <Card>
                <Table
                    columns={columns}
                    data={sales}
                    loading={loading}
                    emptyMessage={t('no_records_found')}
                    keyField="id"
                />
            </Card>

            {/* Sale Details Modal */}
            {showDetailsModal && selectedSale && (
                <>
                    {/* Modal Backdrop */}
                    <div
                        className="modal-backdrop fade show"
                        onClick={() => setShowDetailsModal(false)}
                    ></div>

                    {/* Modal */}
                    <div
                        className="modal fade show d-block"
                        tabIndex="-1"
                        role="dialog"
                        aria-modal="true"
                    >
                        <div className="modal-dialog modal-lg" style={{ marginTop: '50px' }}>
                            <div
                                className="modal-content"
                                style={{
                                    backgroundColor: '#ffffff',
                                    opacity: '1',
                                    boxShadow: '0 0.5rem 1rem rgba(0, 0, 0, 0.5)'
                                }}
                            >
                                <div className="modal-header">
                                    <h5 className="modal-title">{t('sale_details')} #{selectedSale.id}</h5>
                                    <button
                                        type="button"
                                        className="btn-close"
                                        onClick={() => setShowDetailsModal(false)}
                                    ></button>
                                </div>
                                <div className="modal-body">
                                    {detailsLoading ? (
                                        <div className="text-center py-4">
                                            <div className="spinner-border" role="status">
                                                <span className="visually-hidden">{t('loading')}</span>
                                            </div>
                                        </div>
                                    ) : saleDetails ? (
                                        <>
                                            {/* Sale Info */}
                                            <div className="row mb-4">
                                                <div className="col-6">
                                                    <strong>{t('date')}:</strong>{' '}
                                                    {formatDateTime(saleDetails.created_at)}
                                                </div>
                                                <div className="col-6">
                                                    <strong>{t('status')}:</strong>{' '}
                                                    <span className={`badge ${getStatusBadgeClass(saleDetails.status)}`}>
                                                        {saleDetails.status ? t(saleDetails.status.toLowerCase()) : '-'}
                                                    </span>
                                                </div>
                                                <div className="col-6">
                                                    <strong>{t('cashier')}:</strong>{' '}
                                                    {saleDetails.staff_name ? (
                                                        <span className="text-success">{saleDetails.staff_name}</span>
                                                    ) : (
                                                        <span className="text-muted" style={{ fontStyle: 'italic' }}>
                                                            {t('not_assigned')}
                                                        </span>
                                                    )}
                                                </div>
                                                <div className="col-6">
                                                    <strong>{t('payment_method')}:</strong>{' '}
                                                    {saleDetails.payment_method ?
                                                        t(saleDetails.payment_method.toLowerCase()) :
                                                        t('n_a')}
                                                </div>
                                            </div>

                                            {/* Totals */}
                                            <div className="card mb-4">
                                                <div className="card-body">
                                                    <div className="d-flex justify-content-between mb-2">
                                                        <span>{t('subtotal')}:</span>
                                                        <strong>{formatCurrency(saleDetails.subtotal)}</strong>
                                                    </div>
                                                    <div className="d-flex justify-content-between mb-2">
                                                        <span>{t('tax')}:</span>
                                                        <strong>{formatCurrency(saleDetails.tax)}</strong>
                                                    </div>
                                                    <div className="d-flex justify-content-between mb-2">
                                                        <span>{t('discount_amount')}:</span>
                                                        <strong>{formatCurrency(saleDetails.discount)}</strong>
                                                    </div>
                                                    <hr />
                                                    <div className="d-flex justify-content-between">
                                                        <span className="h5 mb-0">{t('total')}:</span>
                                                        <span className="h5 mb-0">
                                                            {formatCurrency(saleDetails.total)}
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>

                                            {/* Items */}
                                            <h6 className="mb-3">{t('sale_items')}</h6>
                                            {saleDetails.items && saleDetails.items.length > 0 ? (
                                                <table className="table table-sm">
                                                    <thead>
                                                        <tr>
                                                            <th>{t('product')}</th>
                                                            <th className="text-center">{t('qty')}</th>
                                                            <th className="text-end">{t('price')}</th>
                                                            <th className="text-end">{t('total')}</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        {saleDetails.items.map((item, index) => (
                                                            <tr key={index}>
                                                                <td>{item.product_name}</td>
                                                                <td className="text-center">{item.quantity}</td>
                                                                <td className="text-end">
                                                                    {formatCurrency(item.price)}
                                                                </td>
                                                                <td className="text-end">
                                                                    {formatCurrency(item.total)}
                                                                </td>
                                                            </tr>
                                                        ))}
                                                    </tbody>
                                                </table>
                                            ) : (
                                                <p className="text-muted">{t('no_items_found')}</p>
                                            )}

                                            {/* Notes */}
                                            {saleDetails.notes && (
                                                <div className="mt-3">
                                                    <strong>Notes:</strong>
                                                    <p className="text-muted mb-0">{saleDetails.notes}</p>
                                                </div>
                                            )}
                                        </>
                                    ) : (
                                        <p className="text-muted">No details available</p>
                                    )}
                                </div>
                                <div className="modal-footer">
                                    <Button
                                        variant="secondary"
                                        onClick={() => setShowDetailsModal(false)}
                                    >
                                        {t('close')}
                                    </Button>
                                </div>
                            </div>
                        </div>
                    </div>
                </>
            )}
        </div>
    );
}

export default SalesPage;
