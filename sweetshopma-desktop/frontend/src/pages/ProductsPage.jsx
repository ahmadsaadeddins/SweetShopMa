import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert, RestockModal, ProductModal } from '../components/common';
import { useProducts, useCategories } from '../hooks';
import { useApi } from '../services';
import { useAuth } from '../context/AuthContext';
import { formatCurrency } from '../utils';
import { useTranslation } from 'react-i18next';

/**
 * Sanitize filters for API calls
 * Converts empty strings to null so the backend ignores the filter
 * @param {Object} filters - Raw filter values
 * @returns {Object} - Sanitized filters
 */
function sanitizeFilters(filters) {
    var sanitized = {};
    var key;
    for (key in filters) {
        if (filters.hasOwnProperty(key)) {
            var value = filters[key];
            // Convert empty strings to null, keep other truthy values
            sanitized[key] = (value === '' || value === null || value === undefined) ? null : value;
        }
    }
    return sanitized;
}

/**
 * Normalize API response data
 * Handles both array and paginated response formats
 * @param {Array|Object} data - API response
 * @returns {Array} - Normalized array
 */
function normalizeData(data) {
    if (Array.isArray(data)) {
        return data;
    }
    if (data && data.results && Array.isArray(data.results)) {
        return data.results;
    }
    return [];
}

/**
 * ProductsPage Component
 * Product management with search, filter, and CRUD operations
 */
function ProductsPage() {
    const { t } = useTranslation();
    const initialFilters = {
        search: '',
        category: '',
        is_active: '',  // Default to showing all statuses
        low_stock: '',
    };

    const [filters, setFilters] = useState(initialFilters);
    const [showAddModal, setShowAddModal] = useState(false);
    const [editingProduct, setEditingProduct] = useState(null);

    // Restock modal state
    const [showRestockModal, setShowRestockModal] = useState(false);
    const [selectedProductForRestock, setSelectedProductForRestock] = useState(null);

    // Sanitize filters before passing to useProducts
    var sanitizedFilters = sanitizeFilters(filters);

    // Use sanitized filters and normalize the data
    var _useProducts = useProducts(sanitizedFilters);

    var productsData = _useProducts.data;
    var loading = _useProducts.loading;
    var error = _useProducts.error;
    var refetch = _useProducts.refetch;

    // Normalize products data (handle both array and paginated responses)
    var products = normalizeData(productsData);
    var categoriesData = useCategories().data;
    var categories = normalizeData(categoriesData);
    const { deleteProduct } = useApi();

    // Get user auth state
    const { user } = useAuth();
    const canRestock = user?.is_staff === true;

    const columns = [
        { header: t('id'), field: 'id', width: '60px' },
        {
            header: t('name'),
            field: 'name',
            render: (val, row) => (
                <div>
                    <div className="fw-bold">{val}</div>
                    {row.sku && <small className="text-muted">SKU: {row.sku}</small>}
                </div>
            )
        },
        { header: t('category'), field: 'category_name', render: (val) => val || '-' },
        { header: t('price'), field: 'price', render: (val) => formatCurrency(val) },
        {
            header: t('stock'),
            field: 'quantity',
            width: '120px',
            render: (val, row) => (
                <div>
                    <span className={row.is_low_stock ? 'text-danger fw-bold' : 'text-success'}>
                        {parseFloat(val).toFixed(row.unit === 'piece' ? 0 : 3)} {row.unit}
                    </span>
                    {row.is_low_stock && (
                        <span className="badge bg-danger ms-2" title="Low stock alert">
                            <i className="bi bi-exclamation-triangle"></i>
                        </span>
                    )}
                </div>
            )
        },
        {
            header: t('status'),
            field: 'is_active',
            width: '100px',
            render: (val) => (
                <span className={`badge ${val ? 'bg-success' : 'bg-secondary'}`}>
                    {val ? t('active') : t('inactive')}
                </span>
            ),
        },
        {
            header: t('actions'),
            field: 'id',
            width: canRestock ? '200px' : '140px',
            render: (val, row) => (
                <div className="d-flex gap-2">
                    {canRestock && (
                        <Button
                            size="small"
                            variant="outline-primary"
                            onClick={() => handleRestockClick(row)}
                            title={t('restock_product')}
                        >
                            <i className="bi bi-plus-circle"></i>
                        </Button>
                    )}
                    <Button
                        size="small"
                        variant="outline-secondary"
                        onClick={() => handleEdit(row)}
                        title={t('edit_product')}
                    >
                        <i className="bi bi-pencil"></i>
                    </Button>
                    <Button
                        size="small"
                        variant="outline-danger"
                        onClick={() => handleDelete(val)}
                        title={t('delete_product')}
                    >
                        <i className="bi bi-trash"></i>
                    </Button>
                </div>
            ),
        },
    ];

    const handleFilterChange = (field, value) => {
        setFilters(prev => ({ ...prev, [field]: value }));
    };

    const handleClearFilters = () => {
        setFilters(initialFilters);
    };

    const handleEdit = (product) => {
        setEditingProduct(product);
        setShowAddModal(true);
    };

    const handleDelete = async (id) => {
        if (!window.confirm(t('delete_confirm_product'))) {
            return;
        }

        try {
            await deleteProduct(id);
            refetch();
        } catch (error) {
            alert(t('error_deleting_product', { error: error.message }));
        }
    };

    const handleAddNew = () => {
        setEditingProduct(null);
        setShowAddModal(true);
    };

    const handleSuccess = () => {
        refetch();
        setShowAddModal(false);
    };

    // Handle restock button click
    const handleRestockClick = (product) => {
        setSelectedProductForRestock(product);
        setShowRestockModal(true);
    };

    // Handle successful restock
    const handleRestockSuccess = () => {
        refetch();
    };

    return (
        <div className="p-4">
            {/* Header */}
            <div className="d-flex justify-content-between align-items-center mb-4">
                <div>
                    <h1 className="h3 mb-1 fw-bold text-primary">{t('products')}</h1>
                    <p className="text-muted mb-0">{t('inventory_management')}</p>
                </div>
                <Button variant="primary" onClick={handleAddNew} className="shadow-sm">
                    <i className="bi bi-plus-lg me-2"></i>
                    {t('add_product')}
                </Button>
            </div>

            {/* Error Alert */}
            {error && (
                <Alert variant="danger" message={t('error_loading_products', { error: error })} dismissible />
            )}

            {/* Filters Card */}
            <Card className="mb-4 shadow-sm border-0">
                <div className="row g-3 align-items-end">
                    <div className="col-12 col-md-4">
                        <Input
                            label={t('search')}
                            placeholder={t('search_products')}
                            value={filters.search}
                            onChange={(e) => handleFilterChange('search', e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Select
                            label={t('category')}
                            value={filters.category}
                            onChange={(e) => handleFilterChange('category', e.target.value)}
                            options={[{ value: '', label: t('all_categories') }].concat(categories.map(cat => ({
                                value: cat.id,
                                label: cat.name,
                            })))}
                            placeholder={t('all_categories')}
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Select
                            label={t('status')}
                            value={filters.is_active}
                            onChange={(e) => handleFilterChange('is_active', e.target.value)}
                            options={[
                                { value: 'true', label: t('active') },
                                { value: 'false', label: t('inactive') },
                                { value: '', label: t('all') },
                            ]}
                            placeholder={t('all_status')}
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Select
                            label={t('stock_level')}
                            value={filters.low_stock}
                            onChange={(e) => handleFilterChange('low_stock', e.target.value)}
                            options={[
                                { value: '', label: t('all_levels') },
                                { value: 'true', label: t('low_stock_filter') },
                            ]}
                            placeholder={t('all_levels')}
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Button
                            variant="outline-secondary"
                            className="w-100"
                            onClick={handleClearFilters}
                            disabled={JSON.stringify(filters) === JSON.stringify(initialFilters)}
                        >
                            {t('clear')}
                        </Button>
                    </div>
                </div>
            </Card>

            {/* Products Table */}
            <Card className="shadow-sm border-0" padding={false}>
                <Table
                    columns={columns}
                    data={products}
                    loading={loading}
                    emptyMessage={t('no_products_criteria')}
                    keyField="id"
                />
            </Card>

            {/* Add/Edit Modal */}
            <ProductModal
                show={showAddModal}
                onClose={() => setShowAddModal(false)}
                onSuccess={handleSuccess}
                product={editingProduct}
                categories={categories}
            />

            {/* Restock Modal */}
            <RestockModal
                show={showRestockModal}
                onClose={() => setShowRestockModal(false)}
                onSuccess={handleRestockSuccess}
                product={selectedProductForRestock}
            />
        </div>
    );
}

export default ProductsPage;
