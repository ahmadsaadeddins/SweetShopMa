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

/**
 * Get all storage keys
 */
export function getStorageKeys() {
    try {
        const allKeys = Object.keys(localStorage);
        return allKeys.filter(key => key.startsWith(STORAGE_PREFIX));
    } catch (error) {
        console.error('[Storage] Error getting storage keys:', error);
        return [];
    }
}

/**
 * Check if storage is available
 */
export function isStorageAvailable() {
    try {
        const testKey = '__storage_test__';
        localStorage.setItem(testKey, 'test');
        localStorage.removeItem(testKey);
        return true;
    } catch (error) {
        return false;
    }
}

/**
 * Get storage usage (approximate)
 */
export function getStorageSize() {
    try {
        let total = 0;
        const keys = Object.keys(localStorage);

        for (const key of keys) {
            if (key.startsWith(STORAGE_PREFIX)) {
                const value = localStorage.getItem(key);
                total += key.length + value.length;
            }
        }

        return total; // in bytes
    } catch (error) {
        console.error('[Storage] Error calculating storage size:', error);
        return 0;
    }
}

/**
 * Format storage size to human readable
 */
export function formatStorageSize(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
}

// Session-specific storage keys
export const STORAGE_KEYS = {
    AUTH_TOKEN: 'auth_token',
    USER_INFO: 'user_info',
    THEME: 'theme',
    CART: 'cart',
    RECENT_PRODUCTS: 'recent_products',
    FILTERS: 'filters',
};
