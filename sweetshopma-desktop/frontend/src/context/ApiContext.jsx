/**
 * ApiContext.jsx - Singleton API Context for PyWebView
 * 
 * Provides a single shared state for API readiness, eliminating
 * redundant polling when multiple hooks initialize simultaneously.
 */

import { createContext, useContext, useState, useEffect, useCallback, useRef, useMemo } from 'react';

const ApiContext = createContext(null);

/**
 * API Provider component that manages shared API state
 */
export function ApiProvider({ children }) {
    const [isReady, setIsReady] = useState(false);
    const [error, setError] = useState(null);
    const [isPolling, setIsPolling] = useState(false);
    const [apiMethods, setApiMethods] = useState([]);

    const pollCountRef = useRef(0);
    const maxPolls = 100; // 100 * 100ms = 10 seconds timeout
    const pollInterval = 100; // Check every 100ms
    const intervalIdRef = useRef(null);

    // Poll for PyWebView API availability
    const pollForApi = useCallback(() => {
        pollCountRef.current += 1;

        if (window.pywebview && window.pywebview.api) {
            const methods = Object.keys(window.pywebview.api);
            console.log('[ApiContext] ✓✓✓ PyWebView API DETECTED! ✓✓✓');
            console.log('[ApiContext] API methods available:', methods.length);
            setApiMethods(methods);
            setIsReady(true);
            setIsPolling(false);
            setError(null);
            return true;
        }

        if (pollCountRef.current >= maxPolls) {
            console.error('[ApiContext] ❌ Timeout: PyWebView API not available after 10 seconds');
            setError('PyWebView API initialization timeout (10s). Are you running in browser mode?');
            setIsPolling(false);
            return false;
        }

        return false;
    }, []);

    // Initialize API detection
    useEffect(() => {
        console.log('[ApiContext] 🔍 Starting PyWebView API detection...');

        // Try immediate check first
        if (pollForApi()) {
            console.log('[ApiContext] ✓ API found immediately');
            return;
        }

        // Start polling
        setIsPolling(true);
        console.log('[ApiContext] API not ready, starting polling...');

        intervalIdRef.current = setInterval(() => {
            const found = pollForApi();
            if (found) {
                console.log('[ApiContext] ✓ API found during polling, clearing interval');
                clearInterval(intervalIdRef.current);
            }
        }, pollInterval);

        // Cleanup on unmount
        return () => {
            console.log('[ApiContext] 🧹 Cleaning up polling interval');
            if (intervalIdRef.current) {
                clearInterval(intervalIdRef.current);
            }
            setIsPolling(false);
        };
    }, [pollForApi]);

    // Wrapper for PyWebView API calls
    const callApi = useCallback(async (methodName, ...args) => {
        if (!isReady) {
            const errorMsg = isPolling
                ? 'PyWebView API not ready (still initializing...)'
                : 'PyWebView API not ready';
            console.error(`[ApiContext] ❌ ${errorMsg}`);
            throw new Error(errorMsg);
        }

        // Check if the method exists
        if (typeof window.pywebview.api[methodName] !== 'function') {
            const errorMsg = `PyWebView API method '${methodName}' not found`;
            console.error(`[ApiContext] ❌ ${errorMsg}`);
            throw new Error(errorMsg);
        }

        try {
            console.log(`[ApiContext] 📞 Calling ${methodName}`);
            if (args.length > 0) {
                console.log(`[ApiContext] Arguments:`, JSON.stringify(args, null, 2));
            }

            const result = await window.pywebview.api[methodName](...args);
            console.log(`[ApiContext] ✓ ${methodName} completed`);

            if (result && typeof result === 'object') {
                console.log(`[ApiContext] Response:`, JSON.stringify(result, null, 2));
            }

            if (result && result.error) {
                throw new Error(result.error);
            }

            return result;
        } catch (err) {
            console.error(`[ApiContext] ❌ Error calling ${methodName}:`, err.message);
            throw err;
        }
    }, [isReady, isPolling]);

    // Handle paginated vs direct array responses
    const normalizeResponse = useCallback((response) => {
        if (Array.isArray(response)) {
            return response;
        }
        if (response && response.results && Array.isArray(response.results)) {
            return response.results;
        }
        return [];
    }, []);

    const value = useMemo(() => ({
        isReady,
        error,
        isPolling,
        apiMethods,
        callApi,
        normalizeResponse,
    }), [isReady, error, isPolling, apiMethods, callApi, normalizeResponse]);

    return (
        <ApiContext.Provider value={value}>
            {children}
        </ApiContext.Provider>
    );
}

/**
 * Custom hook to use the API context
 * Replaces individual usePyWebView() calls
 */
export function useApiContext() {
    const context = useContext(ApiContext);
    if (!context) {
        throw new Error('useApiContext must be used within an ApiProvider');
    }
    return context;
}

/**
 * Legacy wrapper for backward compatibility with usePyWebView
 * Uses shared context instead of creating new polling instance
 */
export function usePyWebView() {
    const { isReady, error, isPolling, callApi } = useApiContext();

    // Log hook usage (for debugging)
    useEffect(() => {
        console.log('[usePyWebView] Hook called - using shared context');
        console.log('[usePyWebView] isReady:', isReady, '| isPolling:', isPolling);
    }, [isReady, isPolling]);

    return {
        isReady,
        error,
        isPolling,
        callApi
    };
}
