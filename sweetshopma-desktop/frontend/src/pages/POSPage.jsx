import React, { useState, useEffect } from 'react';
import { Card, Button, Input, Alert } from '../components/common';
import { RestockModal } from '../components/common';
import { useProducts, useCategories } from '../hooks';
import { useApi } from '../services';
import { useAuth } from '../context/AuthContext';
import { formatCurrency } from '../utils';

/**
 * POSPage Component
 * Point of Sale interface with product grid and cart
 */
function POSPage() {
    const [cart, setCart] = useState([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [selectedCategory, setSelectedCategory] = useState('');
    const [discount, setDiscount] = useState(0);
    const [paymentMethod, setPaymentMethod] = useState('cash');
    const [showCheckout, setShowCheckout] = useState(false);
    const [localProducts, setLocalProducts] = useState(null);

    // Restock modal state
    const [showRestockModal, setShowRestockModal] = useState(false);
    const [selectedProductForRestock, setSelectedProductForRestock] = useState(null);

    // Get user auth state
    const { user } = useAuth();
    const canRestock = user?.is_staff === true;

    const { data: products, loading, refetch: refetchProducts } = useProducts({
        search: searchTerm || undefined,
        category: selectedCategory || undefined,
        is_active: 'true',
    });
    const { data: categories } = useCategories();
    const { createSale } = useApi();

    // Sync local products with API data
    useEffect(() => {
        if (products) {
            setLocalProducts(products.map(p => ({
                ...p,
                quantity: parseFloat(p.quantity)
            })));
        }
    }, [products]);

    // Cart calculations
    const subtotal = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const tax = subtotal * 0; // No tax for now
    const discountAmount = subtotal * (discount / 100);
    const total = subtotal - discountAmount + tax;

    const addToCart = (product, qtyToAdd = 1) => {
        // Check if product is out of stock (only for positive additions)
        if (qtyToAdd > 0 && product.quantity <= 0) {
            alert('This product is out of stock!');
            return;
        }

        setCart(prevCart => {
            const existingItem = prevCart.find(item => item.id === product.id);

            if (existingItem) {
                const newQty = parseFloat((parseFloat(existingItem.quantity) + qtyToAdd).toFixed(3));

                // Remove if quantity becomes zero or negative
                if (newQty <= 0) {
                    return prevCart.filter(item => item.id !== product.id);
                }

                // Check if adding more would exceed available stock
                if (qtyToAdd > 0 && newQty > product.quantity) {
                    alert(`Only ${product.quantity} items available in stock!`);
                    return prevCart;
                }

                return prevCart.map(item =>
                    item.id === product.id
                        ? { ...item, quantity: newQty }
                        : item
                );
            }

            // Cannot remove item that doesn't exist
            if (qtyToAdd <= 0) return prevCart;

            return [...prevCart, { ...product, quantity: parseFloat(qtyToAdd.toFixed(3)) }];
        });
    };

    const updateQuantity = (productId, newQuantity) => {
        // Allow empty string for typing
        if (newQuantity === '' || newQuantity === null) {
            setCart(prevCart =>
                prevCart.map(item =>
                    item.id === productId ? { ...item, quantity: '' } : item
                )
            );
            return;
        }

        const parsedQty = parseFloat(newQuantity);

        if (isNaN(parsedQty) || parsedQty <= 0) {
            // If user attempts to set invalid quantity, we can either ignore or remove.
            // For better UX while typing, we might handle this differently, but for now:
            if (parsedQty === 0) {
                removeFromCart(productId);
                return;
            }
            // If NaN (e.g. typing "1."), let it be if handled by input, but here we are receiving values.
            // If we rely on onBlur for final validation, that's better.
            // But for now, let's assume this is called on change or blur.
        }

        // Find the product to check available stock
        const product = localProducts?.find(p => p.id === productId);
        if (product && parsedQty > product.quantity) {
            alert(`Only ${product.quantity} ${product.unit || 'units'} available in stock!`);
            return;
        }

        setCart(prevCart =>
            prevCart.map(item =>
                item.id === productId ? { ...item, quantity: newQuantity } : item
            )
        );
    };

    const handleQuantityChange = (productId, value) => {
        // Allow decimals
        if (value === '' || /^\d*\.?\d*$/.test(value)) {
            updateQuantity(productId, value);
        }
    };

    const handleQuantityBlur = (productId, value) => {
        const parsed = parseFloat(value);
        if (!value || isNaN(parsed) || parsed <= 0) {
            // Reset to 1 or remove? Let's reset to 1 if invalid
            updateQuantity(productId, 1);
        } else {
            updateQuantity(productId, parsed);
        }
    };

    const removeFromCart = (productId) => {
        setCart(prevCart => prevCart.filter(item => item.id !== productId));
    };

    const clearCart = () => {
        setCart([]);
        setDiscount(0);
    };

    const handleCheckout = async () => {
        if (cart.length === 0) {
            alert('Cart is empty!');
            return;
        }

        const saleData = {
            subtotal: subtotal,
            tax: tax,
            discount: discountAmount,
            total: total,
            payment_method: paymentMethod,
            customer: null,
            notes: '',
            items: cart.map(item => ({
                product_id: item.id,
                quantity: item.quantity,
                price: item.price,
            })),
        };

        try {
            await createSale(saleData);

            // Update product quantities locally for instant UI feedback
            if (localProducts) {
                const soldQuantities = {};
                cart.forEach(item => {
                    soldQuantities[item.id] = (soldQuantities[item.id] || 0) + item.quantity;
                });

                const updatedProducts = localProducts.map(product => {
                    const soldQty = soldQuantities[product.id];
                    if (soldQty) {
                        return {
                            ...product,
                            quantity: Math.max(0, product.quantity - soldQty)
                        };
                    }
                    return product;
                });

                setLocalProducts(updatedProducts);
            }

            alert('Sale completed successfully!');
            clearCart();
            setShowCheckout(false);

            // Refresh products in background to ensure sync with backend
            refetchProducts();
        } catch (error) {
            alert(`Error completing sale: ${error.message}`);
        }
    };

    // Handle restock button click
    const handleRestockClick = (product, e) => {
        e.stopPropagation();
        setSelectedProductForRestock(product);
        setShowRestockModal(true);
    };

    // Handle successful restock
    const handleRestockSuccess = () => {
        refetchProducts();
    };

    const ProductGrid = () => {
        if (loading && !localProducts) {
            return (
                <div className="text-center py-5">
                    <div className="spinner-border" role="status">
                        <span className="visually-hidden">Loading...</span>
                    </div>
                </div>
            );
        }

        if (!localProducts || localProducts.length === 0) {
            return (
                <div className="text-center py-5">
                    <p className="text-muted">No products found</p>
                </div>
            );
        }

        return (
            <div className="row g-3">
                {localProducts.map(product => {
                    const isOutOfStock = product.quantity <= 0;
                    const showWeightUI = product.unit === 'kg' && !isOutOfStock;

                    return (
                        <div key={product.id} className="col-6 col-md-4 col-lg-3">
                            <div
                                className={`card h-100 product-card ${isOutOfStock ? 'opacity-50' : ''}`}
                                style={{
                                    cursor: isOutOfStock && !canRestock ? 'not-allowed' : 'pointer',
                                    overflow: 'hidden'
                                }}
                                onClick={() => !isOutOfStock && addToCart(product)}
                            >
                                {/* Low Stock Badge */}
                                {product.is_low_stock && !isOutOfStock && (
                                    <div className="position-absolute top-0 end-0 m-1">
                                        <span className="badge bg-warning text-dark" title="Low stock">
                                            <i className="bi bi-exclamation-triangle me-1"></i>
                                            Low
                                        </span>
                                    </div>
                                )}

                                {/* Out of Stock Badge */}
                                {isOutOfStock && (
                                    <div className="position-absolute top-0 end-0 m-1">
                                        <span className="badge bg-danger">Out of Stock</span>
                                    </div>
                                )}

                                <div className={`card-body ${showWeightUI ? 'p-0' : 'p-2'}`}>
                                    {showWeightUI ? (
                                        <div style={{ display: 'flex', alignItems: 'stretch', justifyContent: 'space-between', height: '100%' }}>
                                            {/* Left: Remove Buttons */}
                                            <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', gap: '4px', width: '36px', flexShrink: 0 }}>
                                                {[50, 100, 250].map(grams => (
                                                    <button
                                                        key={`minus-${grams}`}
                                                        type="button"
                                                        className="btn btn-danger"
                                                        style={{
                                                            width: '100%',
                                                            height: '24px',
                                                            padding: 0,
                                                            display: 'flex',
                                                            justifyContent: 'center',
                                                            alignItems: 'center',
                                                            fontSize: '10px',
                                                            fontWeight: 'bold',
                                                            lineHeight: 1,
                                                            borderTopLeftRadius: 0,
                                                            borderBottomLeftRadius: 0
                                                        }}
                                                        onClick={(e) => {
                                                            e.stopPropagation();
                                                            addToCart(product, -(grams / 1000));
                                                        }}
                                                    >
                                                        -{grams}
                                                    </button>
                                                ))}
                                            </div>

                                            {/* Center: Info */}
                                            <div style={{ flex: 1, minWidth: 0, overflow: 'hidden', display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', padding: '0 4px' }}>
                                                <h6 className="card-title fw-bold text-truncate" title={product.name} style={{ margin: 0, fontSize: '0.9rem', maxWidth: '100%', textAlign: 'center' }}>{product.name}</h6>
                                                <p className="card-text text-muted text-truncate" style={{ margin: 0, fontSize: '0.75rem', maxWidth: '100%', textAlign: 'center' }}>
                                                    {product.category_name || '-'}
                                                </p>
                                                <h6 className="card-text text-primary fw-bold" style={{ margin: 0, fontSize: '1rem', textAlign: 'center' }}>
                                                    {formatCurrency(product.price)}
                                                </h6>
                                                {product.quantity > 0 ? (
                                                    <small className={product.is_low_stock ? 'text-warning' : 'text-success'} style={{ marginTop: '2px', fontSize: '0.75rem', lineHeight: 1, maxWidth: '100%', textAlign: 'center' }}>
                                                        {Number(product.quantity).toFixed(3)} {product.unit}
                                                    </small>
                                                ) : (
                                                    <small className="text-danger fw-bold" style={{ marginTop: '2px', fontSize: '0.75rem', lineHeight: 1, textAlign: 'center' }}>Out of stock</small>
                                                )}
                                            </div>

                                            {/* Right: Add Buttons */}
                                            <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', gap: '4px', width: '36px', flexShrink: 0 }}>
                                                {[50, 100, 250].map(grams => (
                                                    <button
                                                        key={`plus-${grams}`}
                                                        type="button"
                                                        className="btn btn-success"
                                                        style={{
                                                            width: '100%',
                                                            height: '24px',
                                                            padding: 0,
                                                            display: 'flex',
                                                            justifyContent: 'center',
                                                            alignItems: 'center',
                                                            fontSize: '10px',
                                                            fontWeight: 'bold',
                                                            lineHeight: 1,
                                                            borderTopRightRadius: 0,
                                                            borderBottomRightRadius: 0
                                                        }}
                                                        onClick={(e) => {
                                                            e.stopPropagation();
                                                            addToCart(product, grams / 1000);
                                                        }}
                                                    >
                                                        +{grams}
                                                    </button>
                                                ))}
                                            </div>
                                        </div>
                                    ) : (
                                        <div className="text-center">
                                            <h6 className="card-title mb-2">{product.name}</h6>
                                            <p className="card-text text-muted mb-2">
                                                {product.category_name || '-'}
                                            </p>
                                            <h5 className="card-text text-primary mb-0">
                                                {formatCurrency(product.price)}
                                            </h5>
                                            {product.quantity > 0 ? (
                                                <small className={product.is_low_stock ? 'text-warning' : 'text-success'}>
                                                    {product.is_low_stock && (
                                                        <i className="bi bi-exclamation-triangle me-1"></i>
                                                    )}
                                                    In stock: {Number(product.quantity).toFixed(product.unit === 'kg' ? 3 : 0)} {product.unit || 'pcs'}
                                                </small>
                                            ) : (
                                                <small className="text-danger fw-bold">Out of stock</small>
                                            )}
                                        </div>
                                    )}
                                </div>

                                {/* Restock Button - Only visible for staff */}
                                {canRestock && (
                                    <div className="card-footer bg-transparent border-top-0 p-2">
                                        <button
                                            className="btn btn-outline-primary btn-sm w-100"
                                            onClick={(e) => handleRestockClick(product, e)}
                                            title="Restock this product"
                                        >
                                            <i className="bi bi-plus-circle me-1"></i>
                                            Restock
                                        </button>
                                    </div>
                                )}
                            </div>
                        </div>
                    );
                })
                }
            </div >
        );
    };

    return (
        <div className="p-4">
            <div className="row g-4">
                {/* Products Section */}
                <div className="col-12 col-lg-8">
                    {/* Header */}
                    <div className="mb-4">
                        <h1 className="h3 mb-1">Point of Sale</h1>
                        <p className="text-muted mb-0">Select products to add to cart</p>
                    </div>

                    {/* Search and Filter */}
                    <Card className="mb-4">
                        <div className="row g-3">
                            <div className="col-12 col-md-8">
                                <Input
                                    placeholder="Search products..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                />
                            </div>
                            <div className="col-12 col-md-4">
                                <select
                                    className="form-select"
                                    value={selectedCategory}
                                    onChange={(e) => setSelectedCategory(e.target.value)}
                                >
                                    <option value="">All Categories</option>
                                    {categories?.map(cat => (
                                        <option key={cat.id} value={cat.id}>
                                            {cat.name}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        </div>
                    </Card>

                    {/* Product Grid */}
                    <Card>
                        <ProductGrid />
                    </Card>
                </div>

                {/* Cart Section */}
                <div className="col-12 col-lg-4">
                    <Card
                        title="Shopping Cart"
                        subtitle={`${cart.length} item${cart.length !== 1 ? 's' : ''}`}
                        actions={
                            cart.length > 0 && (
                                <Button
                                    size="small"
                                    variant="danger"
                                    onClick={clearCart}
                                >
                                    Clear
                                </Button>
                            )
                        }
                    >
                        {cart.length === 0 ? (
                            <div className="text-center py-5">
                                <p className="text-muted mb-0">Cart is empty</p>
                                <small className="text-muted">
                                    Click on products to add them to the cart
                                </small>
                            </div>
                        ) : (
                            <>
                                {/* Cart Items */}
                                <div className="cart-items mb-3" style={{ maxHeight: '400px', overflowY: 'auto' }}>
                                    {cart.map(item => (
                                        <div
                                            key={item.id}
                                            className="d-flex flex-column mb-3 pb-3 border-bottom"
                                        >
                                            <div className="d-flex justify-content-between align-items-center">
                                                <div className="flex-grow-1">
                                                    <h6 className="mb-1">{item.name}</h6>
                                                    <p className="text-muted mb-0 small">
                                                        {formatCurrency(item.price)} × {item.quantity}
                                                    </p>
                                                </div>
                                                <div className="d-flex align-items-center gap-2">
                                                    <Button
                                                        size="small"
                                                        variant="secondary"
                                                        onClick={() => updateQuantity(item.id, Number(item.quantity) - (item.unit === 'kg' ? 0.1 : 1))}
                                                    >
                                                        -
                                                    </Button>
                                                    <input
                                                        type="text"
                                                        className="form-control form-control-sm text-center mx-2"
                                                        style={{ width: '60px' }}
                                                        value={item.quantity}
                                                        onChange={(e) => handleQuantityChange(item.id, e.target.value)}
                                                        onBlur={(e) => handleQuantityBlur(item.id, e.target.value)}
                                                    />
                                                    <span className="text-muted small me-2">{item.unit || 'pcs'}</span>
                                                    <Button
                                                        size="small"
                                                        variant="secondary"
                                                        onClick={() => updateQuantity(item.id, Number(item.quantity) + (item.unit === 'kg' ? 0.1 : 1))}
                                                    >
                                                        +
                                                    </Button>
                                                    <Button
                                                        size="small"
                                                        variant="danger"
                                                        onClick={() => removeFromCart(item.id)}
                                                    >
                                                        ×
                                                    </Button>
                                                </div>
                                            </div>

                                            {/* Quick Weight Adjustments - REMOVED from Cart */}
                                        </div>
                                    ))}
                                </div>

                                {/* Cart Summary */}
                                <div className="cart-summary">
                                    <div className="d-flex justify-content-between mb-2">
                                        <span>Subtotal:</span>
                                        <strong>{formatCurrency(subtotal)}</strong>
                                    </div>
                                    {tax > 0 && (
                                        <div className="d-flex justify-content-between mb-2">
                                            <span>Tax:</span>
                                            <strong>{formatCurrency(tax)}</strong>
                                        </div>
                                    )}
                                    <div className="d-flex justify-content-between mb-2 align-items-center">
                                        <span>Discount (%):</span>
                                        <input
                                            type="number"
                                            className="form-control form-control-sm"
                                            style={{ width: '80px' }}
                                            value={discount}
                                            onChange={(e) => setDiscount(parseFloat(e.target.value) || 0)}
                                            min="0"
                                            max="100"
                                            step="1"
                                        />
                                    </div>
                                    {discountAmount > 0 && (
                                        <div className="d-flex justify-content-between mb-2 text-success">
                                            <span>Discount:</span>
                                            <strong>-{formatCurrency(discountAmount)}</strong>
                                        </div>
                                    )}
                                    <hr />
                                    <div className="d-flex justify-content-between mb-3">
                                        <span className="h5 mb-0">Total:</span>
                                        <span className="h5 mb-0 text-primary">
                                            {formatCurrency(total)}
                                        </span>
                                    </div>

                                    {!showCheckout ? (
                                        <Button
                                            variant="primary"
                                            size="large"
                                            className="w-100"
                                            onClick={() => setShowCheckout(true)}
                                        >
                                            Proceed to Checkout
                                        </Button>
                                    ) : (
                                        <>
                                            <div className="mb-3">
                                                <label className="form-label">Payment Method</label>
                                                <select
                                                    className="form-select"
                                                    value={paymentMethod}
                                                    onChange={(e) => setPaymentMethod(e.target.value)}
                                                >
                                                    <option value="cash">Cash</option>
                                                    <option value="card">Card</option>
                                                    <option value="mobile">Mobile Payment</option>
                                                </select>
                                            </div>
                                            <div className="d-flex gap-2">
                                                <Button
                                                    variant="secondary"
                                                    className="flex-grow-1"
                                                    onClick={() => setShowCheckout(false)}
                                                >
                                                    Back
                                                </Button>
                                                <Button
                                                    variant="success"
                                                    className="flex-grow-1"
                                                    onClick={handleCheckout}
                                                >
                                                    Complete Sale
                                                </Button>
                                            </div>
                                        </>
                                    )}
                                </div>
                            </>
                        )}
                    </Card>
                </div>
            </div>

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

export default POSPage;
