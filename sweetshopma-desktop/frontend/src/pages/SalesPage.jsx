import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert } from '../components/common';
import { useSales, useSale } from '../hooks';
import { useApi } from '../services';
import { formatCurrency, formatDateTime } from '../utils';

/**
 * SalesPage Component
 * Sales history with filters and details
 */
function SalesPage() {
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
        { header: 'ID', field: 'id', width: '80px' },
        {
            header: 'Date',
            field: 'created_at',
            render: (val) => formatDateTime(val),
        },
        { header: 'Items', field: 'item_count', width: '80px', render: (val) => val || 0 },
        { header: 'Subtotal', field: 'subtotal', render: (val) => formatCurrency(val) },
        { header: 'Tax', field: 'tax', render: (val) => formatCurrency(val) },
        { header: 'Discount', field: 'discount', render: (val) => formatCurrency(val) },
        { header: 'Total', field: 'total', render: (val) => formatCurrency(val) },
        {
            header: 'Payment',
            field: 'payment_method',
            width: '100px',
            render: (val) => val?.charAt(0).toUpperCase() + val?.slice(1) || '-',
        },
        {
            header: 'Status',
            field: 'status',
            width: '100px',
            render: (val) => (
                <span className={`badge bg-${val === 'completed' ? 'success' : val === 'refunded' ? 'danger' : 'warning'}`}>
                    {val?.charAt(0).toUpperCase() + val?.slice(1) || '-'}
                </span>
            ),
        },
        {
            header: 'Actions',
            field: 'id',
            width: '120px',
            render: (val, row) => (
                <div className="d-flex gap-2">
                    <Button
                        size="small"
                        variant="secondary"
                        onClick={() => handleViewDetails(row)}
                    >
                        View
                    </Button>
                    {row.status === 'completed' && (
                        <Button
                            size="small"
                            variant="warning"
                            onClick={() => handleRefund(val)}
                        >
                            Refund
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
        const reason = window.prompt('Enter refund reason:');
        if (!reason) return;

        try {
            await refundSale(id, reason);
            refetch();
            alert('Sale refunded successfully');
        } catch (error) {
            alert(`Error refunding sale: ${error.message}`);
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
                <h1 className="h3 mb-1">Sales</h1>
                <p className="text-muted mb-0">View and manage sales history</p>
            </div>

            {/* Error Alert */}
            {error && (
                <Alert variant="danger" message={`Error loading sales: ${error}`} />
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
                        <Select
                            label="Status"
                            value={filters.status}
                            onChange={(e) => handleFilterChange('status', e.target.value)}
                            options={[
                                { value: '', label: 'All Statuses' },
                                { value: 'completed', label: 'Completed' },
                                { value: 'refunded', label: 'Refunded' },
                                { value: 'cancelled', label: 'Cancelled' },
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
                    emptyMessage="No sales found"
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
                                    <h5 className="modal-title">Sale Details #{selectedSale.id}</h5>
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
                                                <span className="visually-hidden">Loading...</span>
                                            </div>
                                        </div>
                                    ) : saleDetails ? (
                                        <>
                                            {/* Sale Info */}
                                            <div className="row mb-4">
                                                <div className="col-6">
                                                    <strong>Date:</strong>{' '}
                                                    {formatDateTime(saleDetails.created_at)}
                                                </div>
                                                <div className="col-6">
                                                    <strong>Status:</strong>{' '}
                                                    <span className={`badge ${getStatusBadgeClass(saleDetails.status)}`}>
                                                        {saleDetails.status?.charAt(0).toUpperCase() +
                                                            saleDetails.status?.slice(1)}
                                                    </span>
                                                </div>
                                                <div className="col-6">
                                                    <strong>Cashier:</strong>{' '}
                                                    {saleDetails.staff_name ? (
                                                        <span className="text-success">{saleDetails.staff_name}</span>
                                                    ) : (
                                                        <span className="text-muted" style={{ fontStyle: 'italic' }}>
                                                            Not Assigned
                                                        </span>
                                                    )}
                                                </div>
                                                <div className="col-6">
                                                    <strong>Payment Method:</strong>{' '}
                                                    {saleDetails.payment_method ?
                                                        saleDetails.payment_method.charAt(0).toUpperCase() +
                                                        saleDetails.payment_method.slice(1) :
                                                        'N/A'}
                                                </div>
                                            </div>

                                            {/* Totals */}
                                            <div className="card mb-4">
                                                <div className="card-body">
                                                    <div className="d-flex justify-content-between mb-2">
                                                        <span>Subtotal:</span>
                                                        <strong>{formatCurrency(saleDetails.subtotal)}</strong>
                                                    </div>
                                                    <div className="d-flex justify-content-between mb-2">
                                                        <span>Tax:</span>
                                                        <strong>{formatCurrency(saleDetails.tax)}</strong>
                                                    </div>
                                                    <div className="d-flex justify-content-between mb-2">
                                                        <span>Discount:</span>
                                                        <strong>{formatCurrency(saleDetails.discount)}</strong>
                                                    </div>
                                                    <hr />
                                                    <div className="d-flex justify-content-between">
                                                        <span className="h5 mb-0">Total:</span>
                                                        <span className="h5 mb-0">
                                                            {formatCurrency(saleDetails.total)}
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>

                                            {/* Items */}
                                            <h6 className="mb-3">Sale Items</h6>
                                            {saleDetails.items && saleDetails.items.length > 0 ? (
                                                <table className="table table-sm">
                                                    <thead>
                                                        <tr>
                                                            <th>Product</th>
                                                            <th className="text-center">Qty</th>
                                                            <th className="text-end">Price</th>
                                                            <th className="text-end">Total</th>
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
                                                <p className="text-muted">No items found</p>
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
                                        Close
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
