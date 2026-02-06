import React from 'react';
import { Button } from './Button';
import { formatCurrency, formatDateTime } from '../../utils/formatters';

/**
 * SaleDetailModal Component
 * Reusable modal for displaying detailed sale/order information
 */
function SaleDetailModal({ sale, show, onClose }) {
    if (!show || !sale) return null;

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
        <>
            {/* Modal Backdrop */}
            <div
                className="modal-backdrop fade show"
                onClick={onClose}
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
                            <h5 className="modal-title">Sale Details #{sale.id}</h5>
                            <button
                                type="button"
                                className="btn-close"
                                onClick={onClose}
                            ></button>
                        </div>
                        <div className="modal-body">
                            {/* Sale Info */}
                            <div className="row mb-4">
                                <div className="col-6">
                                    <strong>Date:</strong>{' '}
                                    {formatDateTime(sale.created_at)}
                                </div>
                                <div className="col-6">
                                    <strong>Status:</strong>{' '}
                                    <span className={`badge ${getStatusBadgeClass(sale.status)}`}>
                                        {sale.status?.charAt(0).toUpperCase() +
                                            sale.status?.slice(1)}
                                    </span>
                                </div>
                                <div className="col-6">
                                    <strong>Cashier:</strong>{' '}
                                    {sale.staff_name ? (
                                        <span className="text-success">{sale.staff_name}</span>
                                    ) : (
                                        <span className="text-muted" style={{ fontStyle: 'italic' }}>
                                            Not Assigned
                                        </span>
                                    )}
                                </div>
                                <div className="col-6">
                                    <strong>Payment Method:</strong>{' '}
                                    {sale.payment_method ?
                                        sale.payment_method.charAt(0).toUpperCase() +
                                        sale.payment_method.slice(1) :
                                        'N/A'}
                                </div>
                                {sale.customer_name && (
                                    <div className="col-6">
                                        <strong>Customer:</strong>{' '}
                                        {sale.customer_name}
                                    </div>
                                )}
                            </div>

                            {/* Totals */}
                            <div className="card mb-4">
                                <div className="card-body">
                                    <div className="d-flex justify-content-between mb-2">
                                        <span>Subtotal:</span>
                                        <strong>{formatCurrency(sale.subtotal)}</strong>
                                    </div>
                                    <div className="d-flex justify-content-between mb-2">
                                        <span>Tax:</span>
                                        <strong>{formatCurrency(sale.tax)}</strong>
                                    </div>
                                    <div className="d-flex justify-content-between mb-2">
                                        <span>Discount:</span>
                                        <strong>{formatCurrency(sale.discount)}</strong>
                                    </div>
                                    <hr />
                                    <div className="d-flex justify-content-between">
                                        <span className="h5 mb-0">Total:</span>
                                        <span className="h5 mb-0">
                                            {formatCurrency(sale.total)}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            {/* Items */}
                            <h6 className="mb-3">Sale Items</h6>
                            {sale.items && sale.items.length > 0 ? (
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
                                        {sale.items.map((item, index) => (
                                            <tr key={index}>
                                                <td>{item.product_name}</td>
                                                <td className="text-center">{item.quantity}</td>
                                                <td className="text-end">
                                                    {formatCurrency(item.unit_price)}
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
                            {sale.notes && (
                                <div className="mt-3">
                                    <strong>Notes:</strong>
                                    <p className="text-muted mb-0">{sale.notes}</p>
                                </div>
                            )}
                        </div>
                        <div className="modal-footer">
                            <Button
                                variant="secondary"
                                onClick={onClose}
                            >
                                Close
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default SaleDetailModal;
