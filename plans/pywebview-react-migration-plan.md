# PyWebView React Integration & Migration Plan

**Date:** 2025-02-03
**Status:** Planning Phase
**Current Progress:** Phase 1 Complete (Build System)

## Overview

This plan covers three critical phases:
1. **PyWebView Integration** - Update [`main.py`](sweetshopma-desktop/frontend/main.py) to load the React build
2. **Phase 2: Core Infrastructure** - Create API service hooks and utility functions
3. **Phase 3: Page Migration** - Implement full page functionality with real data

---

## Phase 0: PyWebView Integration (IMMEDIATE)

### Objective
Test the React build in PyWebView by updating [`main.py`](sweetshopma-desktop/frontend/main.py) to load `build/index.html` instead of Django templates.

### Current State
- [`main.py`](sweetshopma-desktop/frontend/main.py:186) loads Django templates via `render_template('login.html')`
- [`ApiBridge.get_page_html()`](sweetshopma-desktop/frontend/api.py:355) serves template-based HTML
- React app is built to `frontend/build/` directory
- Build uses ES2015 (ES6) syntax - compatible with modern webview engines

### Required Changes

#### 1. Update [`main.py`](sweetshopma-desktop/frontend/main.py) - Create Window Method

**Location:** Lines 176-199

**Current Implementation:**
```python
def create_window(self):
    """Create PyWebView window"""
    print("[App] Creating application window...")
    
    # Import API bridge
    from api import ApiBridge
    self.api_bridge = ApiBridge()
    
    # Load initial HTML
    from utils import render_template
    html = render_template('login.html', title="Login - SweetShopMa")
    
    # Create window
    self.window = webview.create_window(
        'SweetShopMa Desktop',
        html=html,
        js_api=self.api_bridge,
        width=WINDOW_WIDTH,
        height=WINDOW_HEIGHT,
        min_size=(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT),
        background_color=WINDOW_BACKGROUND_COLOR,
    )
```

**New Implementation:**
```python
def create_window(self):
    """Create PyWebView window with React build"""
    print("[App] Creating application window...")
    
    # Import API bridge
    from api import ApiBridge
    self.api_bridge = ApiBridge()
    
    # Get path to React build directory
    build_dir = Path(__file__).parent / 'build'
    index_html = build_dir / 'index.html'
    
    if not index_html.exists():
        raise FileNotFoundError(
            f"React build not found at {index_html}. "
            "Run 'npm run build' in the frontend directory first."
        )
    
    print(f"[App] Loading React build from: {build_dir}")
    
    # Create window with React app
    self.window = webview.create_window(
        'SweetShopMa Desktop',
        html=str(index_html),  # Load from file instead of template
        js_api=self.api_bridge,
        width=WINDOW_WIDTH,
        height=WINDOW_HEIGHT,
        min_size=(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT),
        background_color=WINDOW_BACKGROUND_COLOR,
    )
    
    print("[App] ✓ Window created with React app")
```

#### 2. Update [`api.py`](sweetshopma-desktop/frontend/api.py) - Remove Template Rendering

**Location:** Lines 355-404

**Action:** Keep [`get_page_html()`](sweetshopma-desktop/frontend/api.py:355) for backward compatibility but mark as deprecated. React Router will handle navigation.

**Add Deprecation Notice:**
```python
def get_page_html(self, page):
    """
    [DEPRECATED] Get HTML for a specific page or component.
    
    This method is kept for backward compatibility during migration.
    React Router now handles all navigation.
    
    Args:
        page (str): Page name or component path
        
    Returns:
        dict: Response with deprecation notice
    """
    print(f"[ApiBridge] ⚠ get_page_html() called for '{page}' - Navigation handled by React Router")
    return {
        'success': True,
        'html': '<p>Navigation handled by React Router</p>',
        'deprecated': True
    }
```

#### 3. Create React-PyWebView Bridge Hook

**New File:** `frontend/src/hooks/usePyWebView.js`

```javascript
/**
 * Custom hook for PyWebView API communication
 * Wraps pywebview.api calls with error handling and loading states
 */

import { useState, useEffect, useCallback } from 'react';

export function usePyWebView() {
    const [isReady, setIsReady] = useState(false);
    const [error, setError] = useState(null);

    // Check if PyWebView API is available
    useEffect(() => {
        if (window.pywebview && window.pywebview.api) {
            console.log('[usePyWebView] ✓ PyWebView API detected');
            setIsReady(true);
        } else {
            console.warn('[usePyWebView] ⚠ PyWebView API not available - running in browser mode');
            setError('PyWebView API not available');
        }
    }, []);

    // Wrapper for PyWebView API calls
    const callApi = useCallback(async (methodName, ...args) => {
        if (!isReady) {
            throw new Error('PyWebView API not ready');
        }

        try {
            console.log(`[usePyWebView] Calling ${methodName} with:`, args);
            const result = await window.pywebview.api[methodName](...args);
            
            if (result && result.error) {
                throw new Error(result.error);
            }
            
            return result;
        } catch (err) {
            console.error(`[usePyWebView] Error calling ${methodName}:`, err);
            throw err;
        }
    }, [isReady]);

    return {
        isReady,
        error,
        callApi
    };
}
```

### Testing Steps

1. **Build React App:**
   ```bash
   cd sweetshopma-desktop/frontend
   npm run build
   ```

2. **Verify Build Output:**
   - Check `build/index.html` exists
   - Check `build/assets/` contains JS and CSS files

3. **Run PyWebView App:**
   ```bash
   cd sweetshopma-desktop
   python frontend/main.py
   ```

4. **Verify:**
   - Window opens with React app
   - Login page displays
   - Browser console shows "PyWebView API detected"
   - No template rendering errors

---

## Phase 2: Core Infrastructure

### Objective
Create reusable API service hooks and utility functions for the React application.

### 2.1 API Service Layer

#### 2.1.1 Create API Service Hook

**New File:** `frontend/src/services/apiService.js`

```javascript
/**
 * Centralized API service for PyWebView backend communication
 * Handles all API calls with error handling, loading states, and retries
 */

import { usePyWebView } from '../hooks/usePyWebView';

// API endpoint mappings
const API_ENDPOINTS = {
    // Products
    PRODUCTS: 'get_products',
    PRODUCT: 'get_product',
    CREATE_PRODUCT: 'create_product',
    UPDATE_PRODUCT: 'update_product',
    DELETE_PRODUCT: 'delete_product',
    LOW_STOCK_PRODUCTS: 'get_low_stock_products',
    OUT_OF_STOCK_PRODUCTS: 'get_out_of_stock_products',
    BULK_UPDATE_QUANTITY: 'bulk_update_quantity',
    
    // Categories
    CATEGORIES: 'get_categories',
    CREATE_CATEGORY: 'create_category',
    UPDATE_CATEGORY: 'update_category',
    DELETE_CATEGORY: 'delete_category',
    
    // Sales
    SALES: 'get_sales',
    SALE: 'get_sale',
    CREATE_SALE: 'create_sale',
    REFUND_SALE: 'refund_sale',
    TODAY_STATS: 'get_today_stats',
    WEEK_STATS: 'get_week_stats',
    MONTH_STATS: 'get_month_stats',
    RECENT_SALES: 'get_recent_sales',
    
    // Customers
    CUSTOMERS: 'get_customers',
    CUSTOMER: 'get_customer',
    CREATE_CUSTOMER: 'create_customer',
    UPDATE_CUSTOMER: 'update_customer',
    DELETE_CUSTOMER: 'delete_customer',
    
    // Expenses
    EXPENSES: 'get_expenses',
    CREATE_EXPENSE: 'create_expense',
    UPDATE_EXPENSE: 'update_expense',
    DELETE_EXPENSE: 'delete_expense',
    EXPENSE_SUMMARY: 'get_expense_summary',
    
    // Dashboard
    DASHBOARD_STATS: 'get_dashboard_stats',
    
    // Restock
    RESTOCK_RECORDS: 'get_restock_records',
    RESTOCK_PRODUCT: 'restock_product',
    
    // Sync
    SYNC_STATUS: 'get_sync_status',
    SYNC_NOW: 'sync_now',
};

/**
 * Custom hook for API operations
 */
export function useApi() {
    const { isReady, callApi } = usePyWebView();

    /**
     * Handle paginated vs direct array responses
     */
    const normalizeResponse = (response) => {
        if (Array.isArray(response)) {
            return response;
        }
        if (response && response.results && Array.isArray(response.results)) {
            return response.results;
        }
        return [];
    };

    /**
     * Generic API call with error handling
     */
    const apiCall = async (endpoint, ...args) => {
        if (!isReady) {
            throw new Error('API not ready');
        }

        try {
            const response = await callApi(endpoint, ...args);
            
            if (response && response.error) {
                throw new Error(response.error);
            }
            
            return response;
        } catch (error) {
            console.error(`[API] Error calling ${endpoint}:`, error);
            throw error;
        }
    };

    // Products API
    const getProducts = async (filters = {}) => {
        const response = await apiCall(API_ENDPOINTS.PRODUCTS, 
            filters.category,
            filters.search,
            filters.is_active,
            filters.low_stock
        );
        return normalizeResponse(response);
    };

    const getProduct = async (id) => {
        return await apiCall(API_ENDPOINTS.PRODUCT, id);
    };

    const createProduct = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_PRODUCT, data);
    };

    const updateProduct = async (id, data) => {
        return await apiCall(API_ENDPOINTS.UPDATE_PRODUCT, id, data);
    };

    const deleteProduct = async (id) => {
        return await apiCall(API_ENDPOINTS.DELETE_PRODUCT, id);
    };

    // Categories API
    const getCategories = async (search = null) => {
        const response = await apiCall(API_ENDPOINTS.CATEGORIES, search);
        return normalizeResponse(response);
    };

    // Sales API
    const getSales = async (filters = {}) => {
        const response = await apiCall(API_ENDPOINTS.SALES,
            filters.start_date,
            filters.end_date,
            filters.status,
            filters.customer
        );
        return normalizeResponse(response);
    };

    const getRecentSales = async (limit = 10) => {
        const response = await apiCall(API_ENDPOINTS.RECENT_SALES, limit);
        return normalizeResponse(response);
    };

    const createSale = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_SALE, data);
    };

    const getTodayStats = async () => {
        return await apiCall(API_ENDPOINTS.TODAY_STATS);
    };

    // Dashboard API
    const getDashboardStats = async () => {
        return await apiCall(API_ENDPOINTS.DASHBOARD_STATS);
    };

    // Expenses API
    const getExpenses = async (filters = {}) => {
        const response = await apiCall(API_ENDPOINTS.EXPENSES,
            filters.start_date,
            filters.end_date,
            filters.category
        );
        return normalizeResponse(response);
    };

    // Sync API
    const getSyncStatus = async () => {
        return await apiCall(API_ENDPOINTS.SYNC_STATUS);
    };

    const syncNow = async () => {
        return await apiCall(API_ENDPOINTS.SYNC_NOW);
    };

    return {
        isReady,
        // Products
        getProducts,
        getProduct,
        createProduct,
        updateProduct,
        deleteProduct,
        // Categories
        getCategories,
        // Sales
        getSales,
        getRecentSales,
        createSale,
        getTodayStats,
        // Dashboard
        getDashboardStats,
        // Expenses
        getExpenses,
        // Sync
        getSyncStatus,
        syncNow,
    };
}
```

#### 2.1.2 Create Data Fetching Hooks

**New File:** `frontend/src/hooks/useApiData.js`

```javascript
/**
 * Custom hooks for data fetching with loading and error states
 */

import { useState, useEffect } from 'react';
import { useApi } from '../services/apiService';

/**
 * Hook for fetching data with loading and error states
 */
export function useApiData(fetcher, dependencies = []) {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        let isMounted = true;

        const fetchData = async () => {
            try {
                setLoading(true);
                setError(null);
                const result = await fetcher();
                if (isMounted) {
                    setData(result);
                }
            } catch (err) {
                if (isMounted) {
                    setError(err.message);
                    console.error('[useApiData] Fetch error:', err);
                }
            } finally {
                if (isMounted) {
                    setLoading(false);
                }
            }
        };

        fetchData();

        return () => {
            isMounted = false;
        };
    }, dependencies);

    return { data, loading, error, refetch: () => fetchData() };
}

/**
 * Hook for products data
 */
export function useProducts(filters = {}) {
    const { getProducts } = useApi();
    return useApiData(() => getProducts(filters), [JSON.stringify(filters)]);
}

/**
 * Hook for categories data
 */
export function useCategories(search = null) {
    const { getCategories } = useApi();
    return useApiData(() => getCategories(search), [search]);
}

/**
 * Hook for sales data
 */
export function useSales(filters = {}) {
    const { getSales } = useApi();
    return useApiData(() => getSales(filters), [JSON.stringify(filters)]);
}

/**
 * Hook for dashboard stats
 */
export function useDashboardStats() {
    const { getDashboardStats } = useApi();
    return useApiData(() => getDashboardStats(), []);
}

/**
 * Hook for recent sales
 */
export function useRecentSales(limit = 10) {
    const { getRecentSales } = useApi();
    return useApiData(() => getRecentSales(limit), [limit]);
}
```

### 2.2 Utility Functions

#### 2.2.1 Create Formatters

**New File:** `frontend/src/utils/formatters.js`

```javascript
/**
 * Utility functions for formatting data
 */

/**
 * Format currency values
 */
export function formatCurrency(amount, currency = 'EGP') {
    if (amount === null || amount === undefined) {
        return '-';
    }
    
    return new Intl.NumberFormat('en-EG', {
        style: 'currency',
        currency: currency,
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    }).format(amount);
}

/**
 * Format date to locale string
 */
export function formatDate(date, options = {}) {
    if (!date) return '-';
    
    const defaultOptions = {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
    };
    
    return new Date(date).toLocaleDateString('en-EG', {
        ...defaultOptions,
        ...options,
    });
}

/**
 * Format date and time
 */
export function formatDateTime(date) {
    if (!date) return '-';
    
    return new Date(date).toLocaleString('en-EG', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
    });
}

/**
 * Format time
 */
export function formatTime(date) {
    if (!date) return '-';
    
    return new Date(date).toLocaleTimeString('en-EG', {
        hour: '2-digit',
        minute: '2-digit',
    });
}

/**
 * Format relative time (e.g., "2 hours ago")
 */
export function formatRelativeTime(date) {
    if (!date) return '-';
    
    const now = new Date();
    const past = new Date(date);
    const diffMs = now - past;
    const diffSecs = Math.floor(diffMs / 1000);
    const diffMins = Math.floor(diffSecs / 60);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);
    
    if (diffSecs < 60) return 'just now';
    if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    
    return formatDate(date);
}

/**
 * Format percentage
 */
export function formatPercentage(value, decimals = 1) {
    if (value === null || value === undefined) {
        return '-';
    }
    
    return `${value.toFixed(decimals)}%`;
}

/**
 * Format number with thousands separator
 */
export function formatNumber(num) {
    if (num === null || num === undefined) {
        return '-';
    }
    
    return new Intl.NumberFormat('en-EG').format(num);
}

/**
 * Truncate text with ellipsis
 */
export function truncateText(text, maxLength = 50) {
    if (!text) return '';
    
    if (text.length <= maxLength) {
        return text;
    }
    
    return text.substring(0, maxLength) + '...';
}
```

#### 2.2.2 Create Validators

**New File:** `frontend/src/utils/validators.js`

```javascript
/**
 * Validation utility functions
 */

/**
 * Validate email address
 */
export function isValidEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

/**
 * Validate phone number (Egyptian format)
 */
export function isValidPhone(phone) {
    // Egyptian phone: 01xxxxxxxxx (11 digits starting with 01)
    const re = /^01[0-9]{9}$/;
    return re.test(phone);
}

/**
 * Validate required field
 */
export function isRequired(value) {
    if (value === null || value === undefined) {
        return false;
    }
    
    if (typeof value === 'string') {
        return value.trim().length > 0;
    }
    
    if (Array.isArray(value)) {
        return value.length > 0;
    }
    
    return true;
}

/**
 * Validate numeric value
 */
export function isNumeric(value) {
    if (value === null || value === undefined || value === '') {
        return false;
    }
    
    return !isNaN(value) && !isNaN(parseFloat(value));
}

/**
 * Validate positive number
 */
export function isPositive(value) {
    return isNumeric(value) && parseFloat(value) > 0;
}

/**
 * Validate non-negative number
 */
export function isNonNegative(value) {
    return isNumeric(value) && parseFloat(value) >= 0;
}

/**
 * Validate minimum length
 */
export function minLength(value, min) {
    if (!value) return false;
    return value.length >= min;
}

/**
 * Validate maximum length
 */
export function maxLength(value, max) {
    if (!value) return true;
    return value.length <= max;
}

/**
 * Validate range
 */
export function isInRange(value, min, max) {
    if (!isNumeric(value)) return false;
    const num = parseFloat(value);
    return num >= min && num <= max;
}
```

#### 2.2.3 Create Storage Utilities

**New File:** `frontend/src/utils/storage.js`

```javascript
/**
 * Local storage utilities with error handling
 */

const STORAGE_PREFIX = 'sweetshopma_';

/**
 * Get item from local storage
 */
export function getStorageItem(key, defaultValue = null) {
    try {
        const item = localStorage.getItem(STORAGE_PREFIX + key);
        return item ? JSON.parse(item) : defaultValue;
    } catch (error) {
        console.error('[Storage] Error reading from storage:', error);
        return defaultValue;
    }
}

/**
 * Set item in local storage
 */
export function setStorageItem(key, value) {
    try {
        localStorage.setItem(STORAGE_PREFIX + key, JSON.stringify(value));
        return true;
    } catch (error) {
        console.error('[Storage] Error writing to storage:', error);
        return false;
    }
}

/**
 * Remove item from local storage
 */
export function removeStorageItem(key) {
    try {
        localStorage.removeItem(STORAGE_PREFIX + key);
        return true;
    } catch (error) {
        console.error('[Storage] Error removing from storage:', error);
        return false;
    }
}

/**
 * Clear all app storage
 */
export function clearStorage() {
    try {
        const keys = Object.keys(localStorage);
        keys.forEach(key => {
            if (key.startsWith(STORAGE_PREFIX)) {
                localStorage.removeItem(key);
            }
        });
        return true;
    } catch (error) {
        console.error('[Storage] Error clearing storage:', error);
        return false;
    }
}
```

### 2.3 Common UI Components

#### 2.3.1 Create Button Component

**New File:** `frontend/src/components/common/Button.jsx`

```javascript
import React from 'react';

/**
 * Reusable button component with variants
 */
export function Button({
    children,
    variant = 'primary',
    size = 'medium',
    disabled = false,
    loading = false,
    onClick,
    type = 'button',
    className = '',
    ...props
}) {
    const baseClasses = 'btn';
    
    const variantClasses = {
        primary: 'btn-primary',
        secondary: 'btn-secondary',
        success: 'btn-success',
        danger: 'btn-danger',
        warning: 'btn-warning',
        ghost: 'btn-ghost',
    };
    
    const sizeClasses = {
        small: 'btn-sm',
        medium: '',
        large: 'btn-lg',
    };
    
    const classes = [
        baseClasses,
        variantClasses[variant],
        sizeClasses[size],
        className,
    ].filter(Boolean).join(' ');
    
    return (
        <button
            type={type}
            className={classes}
            disabled={disabled || loading}
            onClick={onClick}
            {...props}
        >
            {loading ? (
                <span className="btn-spinner" />
            ) : children}
        </button>
    );
}
```

#### 2.3.2 Create Input Component

**New File:** `frontend/src/components/common/Input.jsx`

```javascript
import React from 'react';

/**
 * Reusable input component with label and error handling
 */
export function Input({
    label,
    type = 'text',
    value,
    onChange,
    placeholder = '',
    error = null,
    disabled = false,
    required = false,
    className = '',
    ...props
}) {
    const inputId = `input-${Math.random().toString(36).substr(2, 9)}`;
    
    return (
        <div className={`form-group ${className}`}>
            {label && (
                <label htmlFor={inputId} className="form-label">
                    {label}
                    {required && <span className="text-danger">*</span>}
                </label>
            )}
            <input
                id={inputId}
                type={type}
                value={value}
                onChange={onChange}
                placeholder={placeholder}
                disabled={disabled}
                required={required}
                className={`form-control ${error ? 'is-invalid' : ''}`}
                {...props}
            />
            {error && (
                <div className="invalid-feedback">{error}</div>
            )}
        </div>
    );
}
```

#### 2.3.3 Create Card Component

**New File:** `frontend/src/components/common/Card.jsx`

```javascript
import React from 'react';

/**
 * Card component for grouping related content
 */
export function Card({
    children,
    title = null,
    subtitle = null,
    actions = null,
    className = '',
    padding = true,
}) {
    return (
        <div className={`card ${className}`}>
            {(title || subtitle || actions) && (
                <div className="card-header">
                    <div className="card-title-row">
                        <div>
                            {title && <h5 className="card-title">{title}</h5>}
                            {subtitle && <p className="card-subtitle">{subtitle}</p>}
                        </div>
                        {actions && <div className="card-actions">{actions}</div>}
                    </div>
                </div>
            )}
            <div className={padding ? 'card-body' : 'card-body-no-padding'}>
                {children}
            </div>
        </div>
    );
}
```

#### 2.3.4 Create Table Component

**New File:** `frontend/src/components/common/Table.jsx`

```javascript
import React from 'react';

/**
 * Reusable table component
 */
export function Table({
    columns = [],
    data = [],
    loading = false,
    emptyMessage = 'No data available',
    onRowClick = null,
    keyField = 'id',
}) {
    if (loading) {
        return (
            <div className="table-loading">
                <div className="spinner-border" role="status">
                    <span className="sr-only">Loading...</span>
                </div>
            </div>
        );
    }
    
    if (!data || data.length === 0) {
        return (
            <div className="table-empty">
                <p>{emptyMessage}</p>
            </div>
        );
    }
    
    return (
        <div className="table-responsive">
            <table className="table">
                <thead>
                    <tr>
                        {columns.map((column, index) => (
                            <th key={index} style={{ width: column.width }}>
                                {column.header}
                            </th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {data.map((row, rowIndex) => (
                        <tr
                            key={row[keyField] || rowIndex}
                            onClick={() => onRowClick && onRowClick(row)}
                            className={onRowClick ? 'table-row-clickable' : ''}
                        >
                            {columns.map((column, colIndex) => (
                                <td key={colIndex}>
                                    {column.render
                                        ? column.render(row[column.field], row)
                                        : row[column.field]}
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
```

---

## Phase 3: Page Migration

### Objective
Implement full page functionality with real data integration, replacing placeholder pages.

### 3.1 Login Page

**File:** [`frontend/src/pages/LoginPage.jsx`](sweetshopma-desktop/frontend/src/pages/LoginPage.jsx)

**Current State:** Basic mock login

**Required Changes:**
- Integrate with real authentication API
- Add form validation
- Handle authentication errors
- Store auth token/session
- Redirect to dashboard on success

**Implementation:**
```javascript
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Button } from '../components/common/Button';
import { Input } from '../components/common/Input';
import { isValidEmail, isRequired } from '../utils/validators';

export default function LoginPage() {
    const navigate = useNavigate();
    const { login } = useAuth();
    
    const [formData, setFormData] = useState({
        username: '',
        password: '',
    });
    const [errors, setErrors] = useState({});
    const [loading, setLoading] = useState(false);
    const [apiError, setApiError] = useState('');

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        // Clear field error on change
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
        setApiError('');
    };

    const validate = () => {
        const newErrors = {};
        
        if (!isRequired(formData.username)) {
            newErrors.username = 'Username is required';
        }
        
        if (!isRequired(formData.password)) {
            newErrors.password = 'Password is required';
        }
        
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        if (!validate()) {
            return;
        }
        
        setLoading(true);
        setApiError('');
        
        try {
            await login(formData.username, formData.password);
            navigate('/dashboard');
        } catch (error) {
            setApiError(error.message || 'Login failed. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="login-page">
            <div className="login-container">
                <div className="login-header">
                    <h1>SweetShopMa</h1>
                    <p>Sign in to your account</p>
                </div>
                
                {apiError && (
                    <div className="alert alert-danger">
                        {apiError}
                    </div>
                )}
                
                <form onSubmit={handleSubmit} className="login-form">
                    <Input
                        label="Username"
                        name="username"
                        value={formData.username}
                        onChange={handleChange}
                        placeholder="Enter your username"
                        error={errors.username}
                        required
                        autoFocus
                    />
                    
                    <Input
                        label="Password"
                        type="password"
                        name="password"
                        value={formData.password}
                        onChange={handleChange}
                        placeholder="Enter your password"
                        error={errors.password}
                        required
                    />
                    
                    <Button
                        type="submit"
                        variant="primary"
                        size="large"
                        loading={loading}
                        className="login-button"
                    >
                        Sign In
                    </Button>
                </form>
            </div>
        </div>
    );
}
```

### 3.2 Dashboard Page

**File:** [`frontend/src/pages/DashboardPage.jsx`](sweetshopma-desktop/frontend/src/pages/DashboardPage.jsx)

**Current State:** Placeholder

**Required Features:**
- Today's sales statistics
- Recent sales table
- Low stock alerts
- Sync status indicator
- Quick action buttons

**Implementation:**
```javascript
import React from 'react';
import { Card } from '../components/common/Card';
import { Table } from '../components/common/Table';
import { useDashboardStats, useRecentSales } from '../hooks/useApiData';
import { formatCurrency, formatDateTime } from '../utils/formatters';

export default function DashboardPage() {
    const { data: stats, loading: statsLoading, error: statsError } = useDashboardStats();
    const { data: recentSales, loading: salesLoading } = useRecentSales(10);

    const salesColumns = [
        { header: 'ID', field: 'id', width: '80px' },
        { header: 'Time', field: 'created_at', render: (val) => formatDateTime(val) },
        { header: 'Items', field: 'item_count', width: '80px' },
        { header: 'Total', field: 'total', render: (val) => formatCurrency(val) },
        { header: 'Status', field: 'status', width: '100px' },
    ];

    if (statsError) {
        return (
            <div className="alert alert-danger">
                Error loading dashboard: {statsError}
            </div>
        );
    }

    return (
        <div className="dashboard-page">
            <h1>Dashboard</h1>
            
            {/* Stats Cards */}
            <div className="stats-grid">
                <Card
                    title="Today's Sales"
                    subtitle={stats?.today_sales_count || 0 + ' transactions'}
                >
                    <div className="stat-value">
                        {formatCurrency(stats?.today_sales_total || 0)}
                    </div>
                </Card>
                
                <Card
                    title="Today's Expenses"
                    subtitle={stats?.today_expenses_count || 0 + ' transactions'}
                >
                    <div className="stat-value">
                        {formatCurrency(stats?.today_expenses_total || 0)}
                    </div>
                </Card>
                
                <Card
                    title="Net Revenue"
                    subtitle="Sales - Expenses"
                >
                    <div className="stat-value">
                        {formatCurrency(
                            (stats?.today_sales_total || 0) - 
                            (stats?.today_expenses_total || 0)
                        )}
                    </div>
                </Card>
                
                <Card
                    title="Low Stock Items"
                    subtitle="Need attention"
                >
                    <div className="stat-value warning">
                        {stats?.low_stock_count || 0}
                    </div>
                </Card>
            </div>
            
            {/* Recent Sales */}
            <Card title="Recent Sales">
                <Table
                    columns={salesColumns}
                    data={recentSales}
                    loading={salesLoading}
                    emptyMessage="No sales today"
                    keyField="id"
                />
            </Card>
        </div>
    );
}
```

### 3.3 POS Page

**File:** [`frontend/src/pages/POSPage.jsx`](sweetshopma-desktop/frontend/src/pages/POSPage.jsx)

**Required Features:**
- Product search/filter
- Add to cart
- Cart management (quantity, remove)
- Calculate totals
- Payment method selection
- Complete sale

**Key Components:**
- ProductGrid
- CartPanel
- CheckoutForm

### 3.4 Products Page

**File:** [`frontend/src/pages/ProductsPage.jsx`](sweetshopma-desktop/frontend/src/pages/ProductsPage.jsx)

**Required Features:**
- Product list with search/filter
- Add/Edit product modal
- Delete confirmation
- Low stock indicators
- Category filter

### 3.5 Sales Page

**File:** [`frontend/src/pages/SalesPage.jsx`](sweetshopma-desktop/frontend/src/pages/SalesPage.jsx)

**Required Features:**
- Sales history table
- Date range filter
- Status filter
- Sale details modal
- Refund functionality

### 3.6 Expenses Page

**File:** [`frontend/src/pages/ExpensesPage.jsx`](sweetshopma-desktop/frontend/src/pages/ExpensesPage.jsx)

**Required Features:**
- Expense list
- Add/Edit expense
- Category filter
- Date range filter
- Summary by category

### 3.7 Settings Page

**File:** [`frontend/src/pages/SettingsPage.jsx`](sweetshopma-desktop/frontend/src/pages/SettingsPage.jsx)

**Required Features:**
- Shop settings
- User profile
- Sync configuration
- Data export
- About/info

---

## Implementation Order

### Phase 0: PyWebView Integration (1-2 hours)
1. ✅ Update [`main.py`](sweetshopma-desktop/frontend/main.py) create_window method
2. ✅ Update [`api.py`](sweetshopma-desktop/frontend/api.py) deprecation notice
3. ✅ Create `usePyWebView` hook
4. ✅ Test PyWebView with React build
5. ✅ Verify navigation works
6. ✅ Verify API bridge is accessible

### Phase 2: Core Infrastructure (4-6 hours)
1. ✅ Create API service layer (`apiService.js`)
2. ✅ Create data fetching hooks (`useApiData.js`)
3. ✅ Create utility functions (formatters, validators, storage)
4. ✅ Create common UI components (Button, Input, Card, Table)
5. ✅ Test all hooks and components
6. ✅ Update global styles for new components

### Phase 3: Page Migration (12-16 hours)
1. ✅ Login page - Real authentication
2. ✅ Dashboard page - Real stats and data
3. ✅ POS page - Full cart and checkout
4. ✅ Products page - CRUD operations
5. ✅ Sales page - History and details
6. ✅ Expenses page - CRUD operations
7. ✅ Settings page - Configuration
8. ✅ Test all pages end-to-end
9. ✅ Error handling and edge cases

---

## Testing Strategy

### Unit Testing
- Test all utility functions
- Test API hooks with mock data
- Test formatters with various inputs

### Integration Testing
- Test API calls to Django backend
- Test PyWebView bridge communication
- Test navigation between pages
- Test authentication flow

### End-to-End Testing
- Complete sale workflow
- Product management workflow
- Expense tracking workflow
- Settings changes persistence

---

## Success Criteria

### Phase 0
- [ ] PyWebView loads React app from `build/index.html`
- [ ] React Router navigation works
- [ ] API bridge accessible from React
- [ ] No template rendering errors

### Phase 2
- [ ] All API endpoints accessible via hooks
- [ ] Loading states work correctly
- [ ] Error handling works for all API calls
- [ ] Formatters handle all data types
- [ ] Validators catch invalid inputs
- [ ] Common components render correctly

### Phase 3
- [ ] Login works with real authentication
- [ ] Dashboard displays real data
- [ ] POS can complete sales
- [ ] Products page can CRUD products
- [ ] Sales page shows history
- [ ] Expenses page tracks expenses
- [ ] Settings page saves configuration

---

## Notes

### ES2015 Compatibility
- Build targets ES2015 (ES6) - compatible with modern webview engines
- If issues arise, may need to update PyWebView or system webview component
- All React code uses modern syntax (const, let, arrow functions, etc.)

### API Response Handling
- DRF ViewSets can return paginated `{results: []}` or direct arrays `[]`
- `normalizeResponse()` in `apiService.js` handles both formats
- Always check for both formats when processing API responses

### Error Handling
- All API calls wrapped in try-catch
- User-friendly error messages displayed
- Console logging for debugging
- Loading states prevent duplicate calls

### Performance Considerations
- Lazy loading for page components
- React.memo for expensive components
- Debounced search inputs
- Optimistic UI updates where appropriate

---

## Next Steps

1. **Review and approve this plan**
2. **Switch to Code mode** to implement Phase 0 (PyWebView Integration)
3. **Test Phase 0** before proceeding to Phase 2
4. **Implement Phase 2** (Core Infrastructure)
5. **Implement Phase 3** (Page Migration) incrementally
6. **End-to-end testing** of all features
7. **Documentation** and cleanup

---

**Questions to Resolve:**

1. Authentication: Does the Django backend have a login endpoint? What's the expected response format?
2. User permissions: How should we handle role-based access control in React?
3. Offline mode: Should we implement offline support with local storage?
4. Hardware integration: Do we need printer/cash drawer integration in React or keep it Python-side?
5. Sync service: How should sync status be displayed and managed in React?

**Dependencies:**
- Django backend must be running on `http://127.0.0.1:8000`
- CORS must be enabled in Django settings
- React app must be built (`npm run build`) before testing in PyWebView
