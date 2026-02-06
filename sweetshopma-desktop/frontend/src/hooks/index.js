/**
 * Hooks barrel file
 * Export all custom hooks for easier imports
 */

// Import usePyWebView from shared context (singleton pattern)
// This eliminates redundant polling when multiple hooks initialize
import { usePyWebView } from '../context/ApiContext';

export { usePyWebView };
export {
    useApiData,
    useProducts,
    useProduct,
    useLowStockProducts,
    useOutOfStockProducts,
    useCategories,
    useSales,
    useSale,
    useRecentSales,
    useTodayStats,
    useDashboardStats,
    useExpenses,
    useExpenseSummary,
    useCustomers,
    useSyncStatus,
    useRestockRecords,
} from './useApiData';
