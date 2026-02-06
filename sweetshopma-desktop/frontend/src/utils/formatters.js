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

/**
 * Format sale status
 */
export function formatSaleStatus(status) {
    const statusMap = {
        'completed': 'Completed',
        'refunded': 'Refunded',
        'cancelled': 'Cancelled',
        'pending': 'Pending',
    };

    return statusMap[status] || status;
}

/**
 * Format payment method
 */
export function formatPaymentMethod(method) {
    const methodMap = {
        'cash': 'Cash',
        'card': 'Card',
        'mobile': 'Mobile Payment',
    };

    return methodMap[method] || method;
}
