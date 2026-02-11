import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from './Button';
import { useApi } from '../../services';

/**
 * RestockModal Component
 * Reusable modal for adding stock to products
 * 
 * Usage:
 * <RestockModal
 *     show={showModal}
 *     onClose={() => setShowModal(false)}
 *     onSuccess={handleRestockSuccess}
 *     product={product}
 * />
 */
function RestockModal({ show, onClose, onSuccess, product }) {
    const { t } = useTranslation();
    const [quantity, setQuantity] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(false);
    const { restockProduct } = useApi();

    // Reset state when modal opens
    useEffect(() => {
        if (show) {
            setQuantity('');
            setError(null);
            setSuccess(false);
            setLoading(false);
        }
    }, [show]);

    // Handle quantity input
    const handleQuantityChange = (e) => {
        const value = e.target.value;
        // Allow only positive numbers with up to 3 decimal places
        if (value === '' || /^\d*\.?\d{0,3}$/.test(value)) {
            setQuantity(value);
        }
    };

    // Calculate stock after restock
    const currentStock = product?.quantity ? parseFloat(product.quantity) : 0;
    const addQty = quantity ? parseFloat(quantity) : 0;
    const stockAfter = currentStock + addQty;

    // Validate quantity
    const validateQuantity = () => {
        if (!quantity || parseFloat(quantity) <= 0) {
            setError(t('invalid_quantity_error'));
            return false;
        }
        if (parseFloat(quantity) > 10000) {
            setError(t('max_quantity_error'));
            return false;
        }
        return true;
    };

    // Handle restock submission
    const handleRestock = async () => {
        if (!validateQuantity()) return;

        setLoading(true);
        setError(null);

        try {
            const result = await restockProduct(product.id, parseFloat(quantity));

            if (result && result.error) {
                setError(result.error);
            } else {
                setSuccess(true);
                // Wait a moment then call success callback
                setTimeout(() => {
                    if (onSuccess) {
                        onSuccess(result);
                    }
                    onClose();
                }, 1500);
            }
        } catch (err) {
            setError(t('failed_restock_error'));
            console.error('[RestockModal] Restock error:', err);
        } finally {
            setLoading(false);
        }
    };

    // Handle backdrop click to close
    const handleBackdropClick = (e) => {
        if (e.target === e.currentTarget && !loading) {
            onClose();
        }
    };

    if (!show) return null;

    return (
        <>
            {/* Modal Backdrop */}
            <div
                className="modal-backdrop fade show"
                onClick={handleBackdropClick}
            ></div>

            {/* Modal */}
            <div
                className="modal fade show d-block"
                tabIndex="-1"
                role="dialog"
                aria-modal="true"
            >
                <div
                    className="modal-dialog modal-md"
                    style={{ marginTop: '100px', maxWidth: '500px' }}
                >
                    <div
                        className="modal-content"
                        style={{
                            backgroundColor: '#ffffff',
                            opacity: '1',
                            boxShadow: '0 0.5rem 1rem rgba(0, 0, 0, 0.5)'
                        }}
                    >
                        {/* Header */}
                        <div className="modal-header">
                            <h5 className="modal-title">
                                <i className="bi bi-box-seam me-2"></i>
                                {t('restock_product')}
                            </h5>
                            {!loading && (
                                <button
                                    type="button"
                                    className="btn-close"
                                    onClick={onClose}
                                ></button>
                            )}
                        </div>

                        {/* Body */}
                        <div className="modal-body">
                            {/* Success Message */}
                            {success ? (
                                <div className="text-center py-4">
                                    <div className="text-success mb-3">
                                        <i className="bi bi-check-circle" style={{ fontSize: '3rem' }}></i>
                                    </div>
                                    <h5 className="text-success">{t('restock_successful')}</h5>
                                    <p className="text-muted">
                                        {t('added_unit_to_product', {
                                            count: parseFloat(quantity).toFixed(3),
                                            unit: t(`unit_${product?.unit?.toLowerCase()}`),
                                            name: product?.name
                                        })}
                                    </p>
                                </div>
                            ) : (
                                <>
                                    {/* Product Info */}
                                    <div className="card mb-4">
                                        <div className="card-body">
                                            <div className="d-flex justify-content-between align-items-center">
                                                <div>
                                                    <h6 className="mb-1">{product?.name}</h6>
                                                    <small className="text-muted">
                                                        {product?.category_name && `${t('category_label')}: ${product.category_name}`}
                                                    </small>
                                                </div>
                                                <div className="text-end">
                                                    <small className="text-muted d-block">{t('current_stock_label')}</small>
                                                    <strong className="h5 mb-0">
                                                        {currentStock.toFixed(3)} <small>{t(`unit_${product?.unit?.toLowerCase()}`)}</small>
                                                    </strong>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Stock Preview */}
                                    <div className="alert alert-info mb-4">
                                        <div className="d-flex justify-content-between align-items-center">
                                            <span>{t('stock_after_restock')}</span>
                                            <strong className="h5 mb-0">
                                                {stockAfter.toFixed(3)} <small>{t(`unit_${product?.unit?.toLowerCase()}`)}</small>
                                            </strong>
                                        </div>
                                    </div>

                                    {/* Quantity Input */}
                                    <div className="mb-4">
                                        <label htmlFor="restockQuantity" className="form-label">
                                            {t('quantity_to_add')} <span className="text-danger">*</span>
                                        </label>
                                        <div className="input-group">
                                            <input
                                                type="number"
                                                id="restockQuantity"
                                                className="form-control form-control-lg"
                                                value={quantity}
                                                onChange={handleQuantityChange}
                                                placeholder={t('enter_valid_amount')}
                                                min="0.001"
                                                max="10000"
                                                step="0.001"
                                                disabled={loading}
                                                autoFocus
                                            />
                                            <span className="input-group-text">{t(`unit_${product?.unit?.toLowerCase()}`)}</span>
                                        </div>
                                        <small className="text-muted">
                                            {t('max_label')} 10,000 {t(`unit_${product?.unit?.toLowerCase()}`)}
                                        </small>
                                    </div>

                                    {/* Error Message */}
                                    {error && (
                                        <div className="alert alert-danger mb-3">
                                            <i className="bi bi-exclamation-triangle me-2"></i>
                                            {error}
                                        </div>
                                    )}
                                </>
                            )}
                        </div>

                        {/* Footer */}
                        {!success && (
                            <div className="modal-footer">
                                <Button
                                    variant="secondary"
                                    onClick={onClose}
                                    disabled={loading}
                                >
                                    {t('cancel')}
                                </Button>
                                <Button
                                    variant="success"
                                    onClick={handleRestock}
                                    disabled={loading || !quantity || parseFloat(quantity) <= 0}
                                >
                                    {loading ? (
                                        <>
                                            <span className="spinner-border spinner-border-sm me-2"></span>
                                            {t('restocking')}
                                        </>
                                    ) : (
                                        <>
                                            <i className="bi bi-plus-circle me-2"></i>
                                            {t('restock')}
                                        </>
                                    )}
                                </Button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </>
    );
}

export default RestockModal;
