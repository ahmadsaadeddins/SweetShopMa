/**
 * Custom hooks for data fetching with loading and error states
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { useApi } from '../services/apiService';

/**
 * Hook for fetching data with loading and error states
 */
export function useApiData(fetcher, dependencies = []) {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Use ref to store the latest fetcher function
    const fetcherRef = useRef(fetcher);
    fetcherRef.current = fetcher;

    const fetchData = useCallback(async () => {
        let isMounted = true;

        try {
            setLoading(true);
            setError(null);
            console.log('[useApiData] Calling fetcher...');
            const result = await fetcherRef.current();
            if (isMounted) {
                console.log('[useApiData] ✓ Fetch successful');
                console.log('[useApiData] Data:', JSON.stringify(result, null, 2));
                setData(result);
            }
        } catch (err) {
            if (isMounted) {
                console.error('[useApiData] ✗ Fetch error:', err.message);
                setError(err.message);
            }
        } finally {
            if (isMounted) {
                setLoading(false);
            }
        }
    }, []); // Empty deps - fetcherRef.current always has the latest fetcher

    useEffect(() => {
        fetchData();
    }, dependencies); // Re-run when dependencies change

    return { data, loading, error, refetch: fetchData };
}

/**
 * Hook for products data
 */
export function useProducts(filters = {}) {
    const { getProducts, isReady } = useApi();
    console.log('[useProducts] isReady:', isReady);

    // Sanitize filters: remove empty/falsy values
    const sanitizedFilters = {};
    for (const key in filters) {
        const value = filters[key];
        // Only include if value is truthy, or explicitly 0 or false
        if (value === 0 || value === false || (value && value !== '')) {
            sanitizedFilters[key] = value;
        }
    }

    const fetcher = async () => {
        if (!isReady) {
            console.log('[useProducts] ❌ API not ready, returning null');
            return null;  // Return null instead of throwing to avoid setting error state
        }
        console.log('[useProducts] ✓ API ready, calling getProducts');
        const result = await getProducts(sanitizedFilters);
        console.log('[useProducts] ✓ Products fetched:', result ? result.length : 0, 'items');
        return result;
    };
    return useApiData(fetcher, [isReady, JSON.stringify(sanitizedFilters)]);
}

/**
 * Hook for a single product
 */
export function useProduct(id) {
    const { getProduct } = useApi();
    return useApiData(() => getProduct(id), [id]);
}

/**
 * Hook for low stock products
 */
export function useLowStockProducts() {
    const { getLowStockProducts, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useLowStockProducts] ❌ API not ready, returning null');
            return null;
        }
        return await getLowStockProducts();
    };
    return useApiData(fetcher, [isReady]);
}

/**
 * Hook for out of stock products
 */
export function useOutOfStockProducts() {
    const { getOutOfStockProducts, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useOutOfStockProducts] ❌ API not ready, returning null');
            return null;
        }
        return await getOutOfStockProducts();
    };
    return useApiData(fetcher, [isReady]);
}

/**
 * Hook for categories data
 */
export function useCategories(search = null) {
    const { getCategories, isReady } = useApi();
    console.log('[useCategories] isReady:', isReady);
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useCategories] ❌ API not ready, returning null');
            return null;  // Return null instead of throwing to avoid setting error state
        }
        console.log('[useCategories] ✓ API ready, calling getCategories');
        return await getCategories(search);
    };
    return useApiData(fetcher, [isReady, search]);
}

/**
 * Hook for sales data
 */
export function useSales(filters = {}) {
    const { getSales, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useSales] ❌ API not ready, returning null');
            return null;
        }
        return await getSales(filters);
    };
    return useApiData(fetcher, [isReady, JSON.stringify(filters)]);
}

/**
 * Hook for a single sale
 */
export function useSale(id) {
    const { getSale, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useSale] ❌ API not ready, returning null');
            return null;
        }
        if (!id) {
            console.log('[useSale] ID is missing, skipping fetch');
            return null;
        }
        return await getSale(id);
    };
    return useApiData(fetcher, [isReady, id]);
}

/**
 * Hook for recent sales
 */
export function useRecentSales(limit = 10) {
    const { getRecentSales, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useRecentSales] ❌ API not ready, returning null');
            return null;
        }
        return await getRecentSales(limit);
    };
    return useApiData(fetcher, [isReady, limit]);
}

/**
 * Hook for today's stats
 */
export function useTodayStats() {
    const { getTodayStats, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useTodayStats] ❌ API not ready, returning null');
            return null;
        }
        return await getTodayStats();
    };
    return useApiData(fetcher, [isReady]);
}

/**
 * Hook for dashboard stats
 */
export function useDashboardStats() {
    const { getDashboardStats, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useDashboardStats] ❌ API not ready, returning null');
            return null;
        }
        return await getDashboardStats();
    };
    return useApiData(fetcher, [isReady]);
}

/**
 * Hook for expenses data
 */
export function useExpenses(filters = {}) {
    const { getExpenses, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useExpenses] ❌ API not ready, returning null');
            return null;
        }
        return await getExpenses(filters);
    };
    return useApiData(fetcher, [isReady, JSON.stringify(filters)]);
}

/**
 * Hook for expense summary
 */
export function useExpenseSummary(filters = {}) {
    const { getExpenseSummary, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useExpenseSummary] ❌ API not ready, returning null');
            return null;
        }
        return await getExpenseSummary(filters);
    };
    return useApiData(fetcher, [isReady, JSON.stringify(filters)]);
}

/**
 * Hook for customers data
 */
export function useCustomers(search = null) {
    const { getCustomers, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useCustomers] ❌ API not ready, returning null');
            return null;
        }
        return await getCustomers(search);
    };
    return useApiData(fetcher, [isReady, search]);
}

/**
 * Hook for sync status
 */
export function useSyncStatus() {
    const { getSyncStatus, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useSyncStatus] ❌ API not ready, returning null');
            return null;
        }
        return await getSyncStatus();
    };
    return useApiData(fetcher, [isReady]);
}

/**
 * Hook for restock records
 */
export function useRestockRecords(productId = null) {
    const { getRestockRecords, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useRestockRecords] ❌ API not ready, returning null');
            return null;
        }
        const response = await getRestockRecords(productId);
        // Normalize response (handle both array and paginated formats)
        if (Array.isArray(response)) {
            return response;
        }
        if (response && response.results && Array.isArray(response.results)) {
            return response.results;
        }
        return [];
    };
    return useApiData(fetcher, [isReady, productId]);
}

/**
 * Hook for attendance records
 */
export function useAttendanceRecords(filters = {}) {
    const { getAttendanceRecords, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useAttendanceRecords] ❌ API not ready, returning null');
            return null;
        }
        return await getAttendanceRecords(filters);
    };
    return useApiData(fetcher, [isReady, JSON.stringify(filters)]);
}

/**
 * Hook for a single attendance record
 */
export function useAttendanceRecord(id) {
    const { getAttendanceRecord, isReady } = useApi();
    return useApiData(() => getAttendanceRecord(id), [isReady, id]);
}

/**
 * Hook for attendance records summary
 */
export function useAttendanceRecordsSummary(filters = {}) {
    const { getAttendanceRecordsSummary, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useAttendanceRecordsSummary] ❌ API not ready, returning null');
            return null;
        }
        return await getAttendanceRecordsSummary(filters);
    };
    return useApiData(fetcher, [isReady, JSON.stringify(filters)]);
}

/**
 * Hook for attendance summaries (monthly per user)
 */
export function useAttendanceSummaries(month = null, userId = null) {
    const { getAttendanceSummaries, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useAttendanceSummaries] ❌ API not ready, returning null');
            return null;
        }
        return await getAttendanceSummaries(month, userId);
    };
    return useApiData(fetcher, [isReady, month, userId]);
}

/**
 * Hook for attendance expenses
 */
export function useAttendanceExpenses(filters = {}) {
    const { getAttendanceExpenses, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useAttendanceExpenses] ❌ API not ready, returning null');
            return null;
        }
        return await getAttendanceExpenses(filters);
    };
    return useApiData(fetcher, [isReady, JSON.stringify(filters)]);
}

/**
 * Hook for users list
 */
export function useUsers() {
    const { getUsers, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useUsers] ❌ API not ready, returning null');
            return null;
        }
        const response = await getUsers();
        return response;
    };
    return useApiData(fetcher, [isReady]);
}

/**
 * Hook for a single user
 */
export function useUser(id) {
    const { getUser, isReady } = useApi();
    return useApiData(() => getUser(id), [isReady, id]);
}

/**
 * Hook for user salary history
 */
export function useUserSalaryHistory(userId) {
    const { getUserSalaryHistory, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useUserSalaryHistory] ❌ API not ready, returning null');
            return null;
        }
        return await getUserSalaryHistory(userId);
    };
    return useApiData(fetcher, [isReady, userId]);
}

/**
 * Hook for all salary history
 */
export function useAllSalaryHistory(userId = null) {
    const { getAllSalaryHistory, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useAllSalaryHistory] ❌ API not ready, returning null');
            return null;
        }
        return await getAllSalaryHistory(userId);
    };
    return useApiData(fetcher, [isReady, userId]);
}

/**
 * Hook for user activities
 */
export function useUserActivities(userId) {
    const { getUserActivities, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useUserActivities] ❌ API not ready, returning null');
            return null;
        }
        return await getUserActivities(userId);
    };
    return useApiData(fetcher, [isReady, userId]);
}

/**
 * Hook for all activities
 */
export function useAllActivities(filters = {}) {
    const { getAllActivities, isReady } = useApi();
    const fetcher = async () => {
        if (!isReady) {
            console.log('[useAllActivities] ❌ API not ready, returning null');
            return null;
        }
        return await getAllActivities(filters);
    };
    return useApiData(fetcher, [isReady, JSON.stringify(filters)]);
}
