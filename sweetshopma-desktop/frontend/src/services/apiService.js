/**
 * Centralized API service for PyWebView backend communication
 * Handles all API calls with error handling, loading states, and retries
 */

import { useApiContext } from '../context/ApiContext';

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

    // Attendance
    ATTENDANCE_RECORDS: 'get_attendance_records',
    ATTENDANCE_RECORD: 'get_attendance_record',
    CREATE_ATTENDANCE_RECORD: 'create_attendance_record',
    UPDATE_ATTENDANCE_RECORD: 'update_attendance_record',
    DELETE_ATTENDANCE_RECORD: 'delete_attendance_record',
    ATTENDANCE_SUMMARY: 'get_attendance_summary',
    ATTENDANCE_RECORDS_SUMMARY: 'get_attendance_records_summary',
    ATTENDANCE_BULK_DELETE: 'bulk_delete_attendance_records',
    ATTENDANCE_SUMMARIES: 'get_attendance_summaries',
    ATTENDANCE_EXPENSES: 'get_attendance_expenses',
    CREATE_ATTENDANCE_EXPENSE: 'create_attendance_expense',
    DELETE_ATTENDANCE_EXPENSE: 'delete_attendance_expense',

    // Users
    USERS: 'get_users',
    USER: 'get_user',
    CREATE_USER: 'create_user',
    UPDATE_USER: 'update_user',
    DELETE_USER: 'delete_user',
    USER_SALARY_HISTORY: 'get_user_salary_history',
    ALL_SALARY_HISTORY: 'get_all_salary_history',
    USER_ACTIVITIES: 'get_user_activities',
    ALL_ACTIVITIES: 'get_all_activities',

    // Sync
    SYNC_STATUS: 'get_sync_status',
    SYNC_NOW: 'sync_now',
};

/**
 * Custom hook for API operations
 */
export function useApi() {
    const { isReady, isPolling, callApi, normalizeResponse } = useApiContext();

    /**
     * Generic API call with error handling
     */
    const apiCall = async (endpoint, ...args) => {
        console.log(`[API] 🔍 Calling ${endpoint}`);
        console.log(`[API] isReady: ${isReady}, isPolling: ${isPolling}`);

        if (!isReady) {
            const errorMsg = isPolling
                ? 'API not ready (still initializing...)'
                : 'API not ready';
            console.error(`[API] ❌ ${errorMsg} - endpoint: ${endpoint}`);
            throw new Error(errorMsg);
        }

        try {
            const response = await callApi(endpoint, ...args);

            if (response && response.error) {
                throw new Error(response.error);
            }

            return response;
        } catch (error) {
            console.error(`[API] ❌ Error calling ${endpoint}:`, error.message);
            throw error;
        }
    };

    // Products API
    const getProducts = async (filters = {}) => {
        // If search is empty string, pass null to avoid triggering search logic on backend
        const searchTerm = filters.search ? filters.search : null;

        // Only pass category if it exists and is not empty
        const category = (filters.category && filters.category !== '') ? filters.category : null;

        const response = await apiCall(API_ENDPOINTS.PRODUCTS,
            category,
            searchTerm,
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

    const getLowStockProducts = async () => {
        const response = await apiCall(API_ENDPOINTS.LOW_STOCK_PRODUCTS);
        return normalizeResponse(response);
    };

    const getOutOfStockProducts = async () => {
        const response = await apiCall(API_ENDPOINTS.OUT_OF_STOCK_PRODUCTS);
        return normalizeResponse(response);
    };

    const bulkUpdateQuantity = async (updates) => {
        return await apiCall(API_ENDPOINTS.BULK_UPDATE_QUANTITY, updates);
    };

    // Categories API
    const getCategories = async (search = null) => {
        const response = await apiCall(API_ENDPOINTS.CATEGORIES, search);
        return normalizeResponse(response);
    };

    const createCategory = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_CATEGORY, data);
    };

    const updateCategory = async (id, data) => {
        return await apiCall(API_ENDPOINTS.UPDATE_CATEGORY, id, data);
    };

    const deleteCategory = async (id) => {
        return await apiCall(API_ENDPOINTS.DELETE_CATEGORY, id);
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

    const getSale = async (id) => {
        return await apiCall(API_ENDPOINTS.SALE, id);
    };

    const createSale = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_SALE, data);
    };

    const refundSale = async (id, reason = '') => {
        return await apiCall(API_ENDPOINTS.REFUND_SALE, id, reason);
    };

    const getTodayStats = async () => {
        return await apiCall(API_ENDPOINTS.TODAY_STATS);
    };

    const getWeekStats = async () => {
        return await apiCall(API_ENDPOINTS.WEEK_STATS);
    };

    const getMonthStats = async () => {
        return await apiCall(API_ENDPOINTS.MONTH_STATS);
    };

    const getRecentSales = async (limit = 10) => {
        const response = await apiCall(API_ENDPOINTS.RECENT_SALES, limit);
        return normalizeResponse(response);
    };

    // Customers API
    const getCustomers = async (search = null) => {
        const response = await apiCall(API_ENDPOINTS.CUSTOMERS, search);
        return normalizeResponse(response);
    };

    const getCustomer = async (id) => {
        return await apiCall(API_ENDPOINTS.CUSTOMER, id);
    };

    const createCustomer = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_CUSTOMER, data);
    };

    const updateCustomer = async (id, data) => {
        return await apiCall(API_ENDPOINTS.UPDATE_CUSTOMER, id, data);
    };

    const deleteCustomer = async (id) => {
        return await apiCall(API_ENDPOINTS.DELETE_CUSTOMER, id);
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

    const createExpense = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_EXPENSE, data);
    };

    const updateExpense = async (id, data) => {
        return await apiCall(API_ENDPOINTS.UPDATE_EXPENSE, id, data);
    };

    const deleteExpense = async (id) => {
        return await apiCall(API_ENDPOINTS.DELETE_EXPENSE, id);
    };

    const getExpenseSummary = async (filters = {}) => {
        return await apiCall(API_ENDPOINTS.EXPENSE_SUMMARY,
            filters.start_date,
            filters.end_date
        );
    };

    // Dashboard API
    const getDashboardStats = async () => {
        return await apiCall(API_ENDPOINTS.DASHBOARD_STATS);
    };

    // Restock API
    const getRestockRecords = async (productId = null) => {
        return await apiCall(API_ENDPOINTS.RESTOCK_RECORDS, productId);
    };

    const restockProduct = async (productId, quantity) => {
        return await apiCall(API_ENDPOINTS.RESTOCK_PRODUCT, productId, quantity);
    };

    // Sync API
    const getSyncStatus = async () => {
        return await apiCall(API_ENDPOINTS.SYNC_STATUS);
    };

    const syncNow = async () => {
        return await apiCall(API_ENDPOINTS.SYNC_NOW);
    };

    // User API
    const getUsers = async () => {
        const response = await apiCall(API_ENDPOINTS.USERS);
        return normalizeResponse(response);
    };

    const getUser = async (id) => {
        return await apiCall(API_ENDPOINTS.USER, id);
    };

    const createUser = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_USER, data);
    };

    const updateUser = async (id, data) => {
        return await apiCall(API_ENDPOINTS.UPDATE_USER, id, data);
    };

    const deleteUser = async (id) => {
        return await apiCall(API_ENDPOINTS.DELETE_USER, id);
    };

    const getUserSalaryHistory = async (userId) => {
        return await apiCall(API_ENDPOINTS.USER_SALARY_HISTORY, userId);
    };

    const getAllSalaryHistory = async (userId = null) => {
        if (userId) {
            return await apiCall(API_ENDPOINTS.ALL_SALARY_HISTORY, userId);
        }
        return await apiCall(API_ENDPOINTS.ALL_SALARY_HISTORY);
    };

    const getUserActivities = async (userId) => {
        return await apiCall(API_ENDPOINTS.USER_ACTIVITIES, userId);
    };

    const getAllActivities = async (filters = {}) => {
        console.log('[getAllActivities] Called with filters:', filters);
        return await apiCall(API_ENDPOINTS.ALL_ACTIVITIES, filters);
    };

    // Attendance API
    const getAttendanceRecords = async (filters = {}) => {
        const params = new URLSearchParams();
        if (filters.user) params.append('user', filters.user);
        if (filters.start_date) params.append('start_date', filters.start_date);
        if (filters.end_date) params.append('end_date', filters.end_date);
        if (filters.status) params.append('status', filters.status);
        if (filters.is_present !== undefined) params.append('is_present', filters.is_present);
        if (filters.search) params.append('search', filters.search);

        const response = await apiCall(API_ENDPOINTS.ATTENDANCE_RECORDS, params.toString());
        return normalizeResponse(response);
    };

    const getAttendanceRecord = async (id) => {
        return await apiCall(API_ENDPOINTS.ATTENDANCE_RECORD, id);
    };

    const createAttendanceRecord = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_ATTENDANCE_RECORD, data);
    };

    const updateAttendanceRecord = async (id, data) => {
        return await apiCall(API_ENDPOINTS.UPDATE_ATTENDANCE_RECORD, id, data);
    };

    const deleteAttendanceRecord = async (id) => {
        return await apiCall(API_ENDPOINTS.DELETE_ATTENDANCE_RECORD, id);
    };

    const getAttendanceRecordsSummary = async (filters = {}) => {
        const params = new URLSearchParams();
        if (filters.user_id) params.append('user_id', filters.user_id);
        if (filters.start_date) params.append('start_date', filters.start_date);
        if (filters.end_date) params.append('end_date', filters.end_date);

        return await apiCall(API_ENDPOINTS.ATTENDANCE_RECORDS_SUMMARY, params.toString());
    };

    const bulkDeleteAttendanceRecords = async (ids) => {
        return await apiCall(API_ENDPOINTS.ATTENDANCE_BULK_DELETE, { ids: ids });
    };

    const getAttendanceSummaries = async (month = null, userId = null) => {
        const params = new URLSearchParams();
        if (month) params.append('month', month);
        if (userId) params.append('user', userId);

        const response = await apiCall(API_ENDPOINTS.ATTENDANCE_SUMMARIES, params.toString());
        return normalizeResponse(response);
    };

    const getAttendanceExpenses = async (filters = {}) => {
        const params = new URLSearchParams();
        if (filters.user) params.append('user', filters.user);
        if (filters.start_date) params.append('start_date', filters.start_date);
        if (filters.end_date) params.append('end_date', filters.end_date);
        if (filters.category) params.append('category', filters.category);

        const response = await apiCall(API_ENDPOINTS.ATTENDANCE_EXPENSES, params.toString());
        return normalizeResponse(response);
    };

    const createAttendanceExpense = async (data) => {
        return await apiCall(API_ENDPOINTS.CREATE_ATTENDANCE_EXPENSE, data);
    };

    const deleteAttendanceExpense = async (id) => {
        return await apiCall(API_ENDPOINTS.DELETE_ATTENDANCE_EXPENSE, id);
    };

    return {
        isReady,
        // Products
        getProducts,
        getProduct,
        createProduct,
        updateProduct,
        deleteProduct,
        getLowStockProducts,
        getOutOfStockProducts,
        bulkUpdateQuantity,
        // Categories
        getCategories,
        createCategory,
        updateCategory,
        deleteCategory,
        // Sales
        getSales,
        getSale,
        createSale,
        refundSale,
        getTodayStats,
        getWeekStats,
        getMonthStats,
        getRecentSales,
        // Customers
        getCustomers,
        getCustomer,
        createCustomer,
        updateCustomer,
        deleteCustomer,
        // Expenses
        getExpenses,
        createExpense,
        updateExpense,
        deleteExpense,
        getExpenseSummary,
        // Dashboard
        getDashboardStats,
        // Restock
        getRestockRecords,
        restockProduct,
        // Sync
        getSyncStatus,
        syncNow,
        // User
        getUsers,
        getUser,
        createUser,
        updateUser,
        deleteUser,
        getUserSalaryHistory,
        getAllSalaryHistory,
        getUserActivities,
        getAllActivities,
        // Attendance
        getAttendanceRecords,
        getAttendanceRecord,
        createAttendanceRecord,
        updateAttendanceRecord,
        deleteAttendanceRecord,
        getAttendanceRecordsSummary,
        bulkDeleteAttendanceRecords,
        getAttendanceSummaries,
        getAttendanceExpenses,
        createAttendanceExpense,
        deleteAttendanceExpense,
    };
}
