import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from './Button';
import { Input, Select, Textarea, Checkbox } from './Input';
import { useApi } from '../../services';

/**
 * ProductModal Component
 * Modal for creating and editing products
 * 
 * @param {Object} props
 * @param {boolean} props.show - Whether to show the modal
 * @param {Function} props.onClose - Function to call when closing the modal
 * @param {Function} props.onSuccess - Function to call when product is saved successfully
 * @param {Object} props.product - Product data for editing (null for new product)
 * @param {Array} props.categories - List of available categories
 */
function ProductModal({ show, onClose, onSuccess, product, categories }) {
    const { t } = useTranslation();
    const [formData, setFormData] = useState({
        name: '',
        category: '',
        price: '',
        cost: '',
        quantity: '0',
        unit: 'piece',
        low_stock_threshold: '10',
        barcode: '',
        sku: '',
        description: '',
        is_active: true
    });

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const { createProduct, updateProduct } = useApi();

    // Reset/Populate form when modal opens or product changes
    useEffect(() => {
        if (show) {
            if (product) {
                setFormData({
                    name: product.name || '',
                    category: product.category || '',
                    price: product.price || '',
                    cost: product.cost || '',
                    quantity: product.quantity || '0',
                    unit: product.unit || 'piece',
                    low_stock_threshold: product.low_stock_threshold || '10',
                    barcode: product.barcode || '',
                    sku: product.sku || '',
                    description: product.description || '',
                    is_active: product.is_active !== undefined ? product.is_active : true
                });
            } else {
                setFormData({
                    name: '',
                    category: '',
                    price: '',
                    cost: '',
                    quantity: '0',
                    unit: 'piece',
                    low_stock_threshold: '10',
                    barcode: '',
                    sku: '',
                    description: '',
                    is_active: true
                });
            }
            setError(null);
        }
    }, [show, product]);

    const handleChange = (field, value) => {
        setFormData(prev => ({ ...prev, [field]: value }));
    };

    const validate = () => {
        if (!formData.name) return t('error_name_required', 'Product name is required');
        if (!formData.price || parseFloat(formData.price) < 0) return t('error_price_required', 'Valid price is required');
        if (formData.quantity === '' || parseFloat(formData.quantity) < 0) return t('error_quantity_required', 'Valid quantity is required');
        return null;
    };

    const handleSubmit = async (e) => {
        if (e) e.preventDefault();

        const validationError = validate();
        if (validationError) {
            setError(validationError);
            return;
        }

        setLoading(true);
        setError(null);

        try {
            // Prepare data for API (ensure numbers are actually numbers)
            const submissionData = {
                ...formData,
                price: parseFloat(formData.price),
                cost: formData.cost ? parseFloat(formData.cost) : null,
                quantity: parseFloat(formData.quantity),
                low_stock_threshold: parseInt(formData.low_stock_threshold, 10),
                category: formData.category || null
            };

            let result;
            if (product && product.id) {
                result = await updateProduct(product.id, submissionData);
            } else {
                result = await createProduct(submissionData);
            }

            if (onSuccess) {
                onSuccess(result);
            }
            onClose();
        } catch (err) {
            console.error('[ProductModal] Error saving product:', err);
            setError(err.message || t('error_save_failed', 'Failed to save product'));
        } finally {
            setLoading(false);
        }
    };

    const handleBackdropClick = (e) => {
        if (e.target === e.currentTarget && !loading) {
            onClose();
        }
    };

    if (!show) return null;

    const unitOptions = [
        { value: 'piece', label: t('unit_piece') },
        { value: 'kg', label: t('unit_kg') },
        { value: 'g', label: t('unit_g') },
        { value: 'lb', label: t('unit_lb') },
        { value: 'oz', label: t('unit_oz') },
    ];

    const categoryOptions = categories.map(cat => ({
        value: cat.id,
        label: cat.name
    }));

    return (
        <>
            <div className="modal-backdrop fade show" onClick={handleBackdropClick}></div>
            <div className="modal fade show d-block" tabIndex="-1" role="dialog">
                <div className="modal-dialog modal-lg">
                    <div
                        className="modal-content"
                        style={{
                            backgroundColor: '#ffffff',
                            opacity: '1',
                            boxShadow: '0 0.5rem 1rem rgba(0, 0, 0, 0.5)'
                        }}
                    >
                        <div className="modal-header bg-light">
                            <h5 className="modal-title fw-bold">
                                <i className={`bi bi-${product ? 'pencil-square' : 'plus-circle'} me-2 text-primary`}></i>
                                {product ? t('edit_product_title') : t('add_product_title')}
                            </h5>
                            <button
                                type="button"
                                className="btn-close"
                                onClick={onClose}
                                disabled={loading}
                            ></button>
                        </div>
                        <div className="modal-body p-4">
                            {error && (
                                <div className="alert alert-danger d-flex align-items-center mb-4" role="alert">
                                    <i className="bi bi-exclamation-triangle-fill me-2"></i>
                                    <div>{error}</div>
                                </div>
                            )}

                            <form onSubmit={handleSubmit}>
                                <div className="row g-3">
                                    {/* Basic Info Section */}
                                    <div className="col-12 mb-2">
                                        <h6 className="text-uppercase text-muted fw-bold small border-bottom pb-2">{t('basic_info_section')}</h6>
                                    </div>
                                    <div className="col-md-8">
                                        <Input
                                            label={t('product_name_label')}
                                            value={formData.name}
                                            onChange={(e) => handleChange('name', e.target.value)}
                                            placeholder={t('enter_product_name')}
                                            required
                                            disabled={loading}
                                        />
                                    </div>
                                    <div className="col-md-4">
                                        <Select
                                            label={t('category_label')}
                                            value={formData.category}
                                            onChange={(e) => handleChange('category', e.target.value)}
                                            options={categoryOptions}
                                            placeholder={t('none')}
                                            disabled={loading}
                                        />
                                    </div>

                                    {/* Inventory Section */}
                                    <div className="col-12 mt-4 mb-2">
                                        <h6 className="text-uppercase text-muted fw-bold small border-bottom pb-2">{t('pricing_inventory_section')}</h6>
                                    </div>
                                    <div className="col-md-3">
                                        <Input
                                            label={t('selling_price_label')}
                                            type="number"
                                            step="0.01"
                                            value={formData.price}
                                            onChange={(e) => handleChange('price', e.target.value)}
                                            placeholder="0.00"
                                            required
                                            disabled={loading}
                                        />
                                    </div>
                                    <div className="col-md-3">
                                        <Input
                                            label={t('cost_price_label')}
                                            type="number"
                                            step="0.01"
                                            value={formData.cost}
                                            onChange={(e) => handleChange('cost', e.target.value)}
                                            placeholder="0.00"
                                            disabled={loading}
                                        />
                                    </div>
                                    <div className="col-md-3">
                                        <Input
                                            label={t('initial_stock_label')}
                                            type="number"
                                            step="0.001"
                                            value={formData.quantity}
                                            onChange={(e) => handleChange('quantity', e.target.value)}
                                            placeholder="0.000"
                                            disabled={loading || !!product} // Quantity shouldn't be edited directly for existing products, use Restock
                                            helpText={!!product ? t('restock_hint') : t('starting_stock_hint')}
                                        />
                                    </div>
                                    <div className="col-md-3">
                                        <Select
                                            label={t('unit')}
                                            value={formData.unit}
                                            onChange={(e) => handleChange('unit', e.target.value)}
                                            options={unitOptions}
                                            disabled={loading}
                                        />
                                    </div>

                                    {/* Additional Settings Section */}
                                    <div className="col-12 mt-4 mb-2">
                                        <h6 className="text-uppercase text-muted fw-bold small border-bottom pb-2">{t('additional_settings_section')}</h6>
                                    </div>
                                    <div className="col-md-6">
                                        <Input
                                            label={t('barcode_label')}
                                            value={formData.barcode}
                                            onChange={(e) => handleChange('barcode', e.target.value)}
                                            placeholder={t('scan_barcode_placeholder')}
                                            disabled={loading}
                                        />
                                    </div>
                                    <div className="col-md-6">
                                        <Input
                                            label={t('sku_label')}
                                            value={formData.sku}
                                            onChange={(e) => handleChange('sku', e.target.value)}
                                            placeholder={t('enter_sku_placeholder')}
                                            disabled={loading}
                                        />
                                    </div>
                                    <div className="col-md-6">
                                        <Input
                                            label={t('low_stock_threshold_label')}
                                            type="number"
                                            value={formData.low_stock_threshold}
                                            onChange={(e) => handleChange('low_stock_threshold', e.target.value)}
                                            disabled={loading}
                                        />
                                    </div>
                                    <div className="col-md-6 d-flex align-items-center pt-4">
                                        <Checkbox
                                            label={t('product_active_label')}
                                            checked={formData.is_active}
                                            onChange={(e) => handleChange('is_active', e.target.checked)}
                                            disabled={loading}
                                        />
                                    </div>
                                    <div className="col-12">
                                        <Textarea
                                            label={t('description_label')}
                                            value={formData.description}
                                            onChange={(e) => handleChange('description', e.target.value)}
                                            placeholder={t('product_details_placeholder')}
                                            rows={2}
                                            disabled={loading}
                                        />
                                    </div>
                                </div>
                            </form>
                        </div>
                        <div className="modal-footer bg-light border-top-0">
                            <Button
                                variant="secondary"
                                onClick={onClose}
                                disabled={loading}
                            >
                                {t('cancel')}
                            </Button>
                            <Button
                                variant="primary"
                                onClick={handleSubmit}
                                disabled={loading}
                            >
                                {loading ? (
                                    <>
                                        <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                                        {t('saving_btn')}
                                    </>
                                ) : (
                                    <>
                                        <i className="bi bi-check-lg me-2"></i>
                                        {product ? t('update_product_btn') : t('create_product_btn')}
                                    </>
                                )}
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default ProductModal;
