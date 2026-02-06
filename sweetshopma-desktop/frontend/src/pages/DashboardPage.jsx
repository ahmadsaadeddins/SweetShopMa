import React, { useState } from 'react';
import { StatCard, Card, Table, Alert, Button, SaleDetailModal, RestockModal } from '../components/common';
import { useDashboardStats, useRecentSales, useLowStockProducts } from '../hooks';
import { useAuth } from '../context/AuthContext';
import { formatCurrency, formatDateTime } from '../utils';

/**
 * DashboardPage Component
 * Main dashboard with statistics and recent activity
 */
function DashboardPage() {
    const { data: stats, loading: statsLoading, error: statsError } = useDashboardStats();
    const { data: recentSales, loading: salesLoading } = useRecentSales(10);
    const { data: lowStockProducts, loading: stockLoading, refetch: refetchLowStock } = useLowStockProducts();

    const [selectedSale, setSelectedSale] = useState(null);
    const [showDetailsModal, setShowDetailsModal] = useState(false);

    // Restock modal state
    const [showRestockModal, setShowRestockModal] = useState(false);
    const [selectedProductForRestock, setSelectedProductForRestock] = useState(null);

    // Get user auth state
    const { user } = useAuth();
    const canRestock = user?.is_staff === true;

    const handleViewDetails = (saleId) => {
        const sale = recentSales.find(s => s.id === saleId);
        if (sale) {
            setSelectedSale(sale);
            setShowDetailsModal(true);
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

    const salesColumns = [
        { header: 'ID', field: 'id', width: '80px' },
        { header: 'Time', field: 'created_at', render: (val) => formatDateTime(val) },
        {
            header: 'Cashier',
            field: 'staff_name',
            width: '120px',
            render: (val) => val || '-'
        },
        {
            header: 'Items',
            field: 'items_summary',
            width: '200px',
            render: (val, row) => {
                const itemNames = row.items?.map(i => i.product_name) || [];
                if (itemNames.length === 0) return '-';
                const display = itemNames.slice(0, 2).join(', ');
                const more = itemNames.length > 2 ? ` +${itemNames.length - 2} more` : '';
                return `${display}${more}`;
            }
        },
        { header: 'Qty', field: 'item_count', width: '60px', render: (val) => val || 0 },
        { header: 'Total', field: 'total', width: '100px', render: (val) => formatCurrency(val) },
        {
            header: 'Status',
            field: 'status',
            width: '100px',
            render: (val) => (
                <span className={`badge ${getStatusBadgeClass(val)}`}>
                    {val?.charAt(0).toUpperCase() + val?.slice(1) || '-'}
                </span>
            )
        },
        {
            header: 'Actions',
            field: 'id',
            width: '100px',
            render: (val) => (
                <Button
                    size="small"
                    variant="secondary"
                    onClick={(e) => {
                        e.stopPropagation();
                        handleViewDetails(val);
                    }}
                >
                    View
                </Button>
            ),
        },
    ];

    const lowStockColumns = [
        { header: 'Product', field: 'name' },
        { header: 'Category', field: 'category_name', render: (val) => val || '-' },
        { header: 'Stock', field: 'quantity', width: '100px' },
        { header: 'Min Stock', field: 'low_stock_threshold', width: '100px' },
        ...(canRestock ? [{
            header: 'Actions',
            field: 'id',
            width: '100px',
            render: (val, row) => (
                <Button
                    size="small"
                    variant="outline-primary"
                    onClick={(e) => {
                        e.stopPropagation();
                        handleRestockClick(row);
                    }}
                >
                    <i className="bi bi-plus-circle me-1"></i>
                    Restock
                </Button>
            ),
        }] : []),
    ];

    // Handle restock button click
    const handleRestockClick = (product) => {
        setSelectedProductForRestock(product);
        setShowRestockModal(true);
    };

    // Handle successful restock
    const handleRestockSuccess = () => {
        refetchLowStock();
    };

    if (statsError) {
        return (
            <div className="p-4">
                <Alert variant="danger" message={`Error loading dashboard: ${statsError}`} />
            </div>
        );
    }

    return (
        <div className="p-4">
            {/* Header */}
            <div className="mb-4">
                <h1 className="h3 mb-1">Dashboard</h1>
                <p className="text-muted">Welcome to SweetShopMa Desktop</p>
            </div>

            {/* Stats Grid */}
            <div className="row g-3 mb-4">
                <div className="col-12 col-md-6 col-lg-3">
                    <StatCard
                        title="Today's Sales"
                        value={formatCurrency(stats?.today_revenue || 0)}
                        subtitle={`${stats?.sales?.total_sales || 0} transactions`}
                        variant="primary"
                        loading={statsLoading}
                    />
                </div>
                <div className="col-12 col-md-6 col-lg-3">
                    <StatCard
                        title="Today's Profit"
                        value={formatCurrency(stats?.sales?.total_profit || 0)}
                        subtitle="From completed sales"
                        variant="danger"
                        loading={statsLoading}
                    />
                </div>
                <div className="col-12 col-md-6 col-lg-3">
                    <StatCard
                        title="Average Sale"
                        value={formatCurrency(stats?.sales?.average_sale || 0)}
                        subtitle="Per transaction"
                        variant="success"
                        loading={statsLoading}
                    />
                </div>
                <div className="col-12 col-md-6 col-lg-3">
                    <StatCard
                        title="Low Stock Items"
                        value={lowStockProducts?.length || 0}
                        subtitle="Need attention"
                        variant="warning"
                        loading={stockLoading}
                    />
                </div>
            </div>

            {/* Additional Stats Row */}
            <div className="row g-3 mb-4">
                <div className="col-12 col-md-6 col-lg-4">
                    <StatCard
                        title="Week's Sales"
                        value={formatCurrency(stats?.week_revenue || 0)}
                        subtitle="This week"
                        variant="info"
                        loading={statsLoading}
                    />
                </div>
                <div className="col-12 col-md-6 col-lg-4">
                    <StatCard
                        title="Month's Sales"
                        value={formatCurrency(stats?.month_revenue || 0)}
                        subtitle="This month"
                        variant="info"
                        loading={statsLoading}
                    />
                </div>
                <div className="col-12 col-md-6 col-lg-4">
                    <StatCard
                        title="Total Products"
                        value={stats?.inventory?.total_products || 0}
                        subtitle={`${stats?.inventory?.low_stock_products || 0} low stock`}
                        variant="secondary"
                        loading={statsLoading}
                    />
                </div>
            </div>

            {/* Recent Sales */}
            <Card title="Recent Sales" className="mb-4">
                <Table
                    columns={salesColumns}
                    data={recentSales}
                    loading={salesLoading}
                    emptyMessage="No sales today"
                    keyField="id"
                    onRowClick={(row) => handleViewDetails(row.id)}
                />
            </Card>

            {/* Low Stock Alert */}
            {lowStockProducts && lowStockProducts.length > 0 && (
                <Card title="Low Stock Alert" variant="warning">
                    <Table
                        columns={lowStockColumns}
                        data={lowStockProducts}
                        loading={stockLoading}
                        emptyMessage="No low stock items"
                        keyField="id"
                    />
                </Card>
            )}

            {/* Sale Details Modal */}
            <SaleDetailModal
                sale={selectedSale}
                show={showDetailsModal}
                onClose={() => setShowDetailsModal(false)}
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

export default DashboardPage;
