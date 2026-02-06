import React, { useState } from 'react';
import { Card, Table, Button, Input, Select, Alert, RestockModal, ProductModal } from '../components/common';
import { useProducts, useCategories } from '../hooks';
import { useApi } from '../services';
import { useAuth } from '../context/AuthContext';
import { formatCurrency } from '../utils';

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
        { header: 'ID', field: 'id', width: '60px' },
        {
            header: 'Name',
            field: 'name',
            render: (val, row) => (
                <div>
                    <div className="fw-bold">{val}</div>
                    {row.sku && <small className="text-muted">SKU: {row.sku}</small>}
                </div>
            )
        },
        { header: 'Category', field: 'category_name', render: (val) => val || '-' },
        { header: 'Price', field: 'price', render: (val) => formatCurrency(val) },
        {
            header: 'Stock',
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
            header: 'Status',
            field: 'is_active',
            width: '100px',
            render: (val) => (
                <span className={`badge ${val ? 'bg-success' : 'bg-secondary'}`}>
                    {val ? 'Active' : 'Inactive'}
                </span>
            ),
        },
        {
            header: 'Actions',
            field: 'id',
            width: canRestock ? '200px' : '140px',
            render: (val, row) => (
                <div className="d-flex gap-2">
                    {canRestock && (
                        <Button
                            size="small"
                            variant="outline-primary"
                            onClick={() => handleRestockClick(row)}
                            title="Restock this product"
                        >
                            <i className="bi bi-plus-circle"></i>
                        </Button>
                    )}
                    <Button
                        size="small"
                        variant="outline-secondary"
                        onClick={() => handleEdit(row)}
                        title="Edit product"
                    >
                        <i className="bi bi-pencil"></i>
                    </Button>
                    <Button
                        size="small"
                        variant="outline-danger"
                        onClick={() => handleDelete(val)}
                        title="Delete product"
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
        if (!window.confirm('Are you sure you want to delete this product? This action cannot be undone.')) {
            return;
        }

        try {
            await deleteProduct(id);
            refetch();
        } catch (error) {
            alert(`Error deleting product: ${error.message}`);
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
                    <h1 className="h3 mb-1 fw-bold text-primary">Products</h1>
                    <p className="text-muted mb-0">Manage your inventory and stock levels</p>
                </div>
                <Button variant="primary" onClick={handleAddNew} className="shadow-sm">
                    <i className="bi bi-plus-lg me-2"></i>
                    Add Product
                </Button>
            </div>

            {/* Error Alert */}
            {error && (
                <Alert variant="danger" message={`Error loading products: ${error}`} dismissible />
            )}

            {/* Filters Card */}
            <Card className="mb-4 shadow-sm border-0">
                <div className="row g-3 align-items-end">
                    <div className="col-12 col-md-4">
                        <Input
                            label="Search"
                            placeholder="Name, barcode, or SKU..."
                            value={filters.search}
                            onChange={(e) => handleFilterChange('search', e.target.value)}
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Select
                            label="Category"
                            value={filters.category}
                            onChange={(e) => handleFilterChange('category', e.target.value)}
                            options={[{ value: '', label: 'All Categories' }].concat(categories.map(cat => ({
                                value: cat.id,
                                label: cat.name,
                            })))}
                            placeholder="All Categories"
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Select
                            label="Status"
                            value={filters.is_active}
                            onChange={(e) => handleFilterChange('is_active', e.target.value)}
                            options={[
                                { value: 'true', label: 'Active' },
                                { value: 'false', label: 'Inactive' },
                                { value: '', label: 'All' },
                            ]}
                            placeholder="All Status"
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Select
                            label="Stock Level"
                            value={filters.low_stock}
                            onChange={(e) => handleFilterChange('low_stock', e.target.value)}
                            options={[
                                { value: '', label: 'All Levels' },
                                { value: 'true', label: 'Low Stock' },
                            ]}
                            placeholder="All Levels"
                        />
                    </div>
                    <div className="col-12 col-md-2">
                        <Button
                            variant="outline-secondary"
                            className="w-100"
                            onClick={handleClearFilters}
                            disabled={JSON.stringify(filters) === JSON.stringify(initialFilters)}
                        >
                            Clear
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
                    emptyMessage="No products found matching your criteria"
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
