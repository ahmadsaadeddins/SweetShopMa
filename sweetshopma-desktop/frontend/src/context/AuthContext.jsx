import React, { createContext, useContext, useState, useEffect } from 'react';
import * as PropTypes from 'prop-types';

/**
 * Authentication Context
 * Provides authentication state and methods throughout the app
 */
const AuthContext = createContext(null);

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    // Check for existing session on mount
    useEffect(() => {
        checkAuthStatus();
    }, []);

    /**
     * Check authentication status from session storage
     */
    const checkAuthStatus = () => {
        try {
            setIsLoading(true);

            // Check if user data exists in session storage
            const storedUser = sessionStorage.getItem('user');
            const storedToken = sessionStorage.getItem('authToken');

            if (storedUser && storedToken) {
                setUser(JSON.parse(storedUser));
                setIsAuthenticated(true);
                console.log('[AuthContext] User authenticated from session');
            } else {
                setUser(null);
                setIsAuthenticated(false);
                console.log('[AuthContext] No active session found');
            }

            setError(null);
        } catch (err) {
            console.error('[AuthContext] Error checking auth status:', err);
            setError(err.message);
            setUser(null);
            setIsAuthenticated(false);
        } finally {
            setIsLoading(false);
        }
    };

    /**
     * Login with username and password
     */
    const login = async (username, password) => {
        try {
            setIsLoading(true);
            setError(null);

            console.log('[AuthContext] Attempting login for:', username);

            // Call PyWebView API for authentication if method exists
            if (window.pywebview &&
                window.pywebview.api &&
                typeof window.pywebview.api.authenticate_user === 'function') {
                const response = await window.pywebview.api.authenticate_user(username, password);

                if (response.error) {
                    throw new Error(response.error);
                }

                if (response.success && response.user) {
                    // Store user data and token
                    setUser(response.user);
                    setIsAuthenticated(true);

                    // Save to session storage
                    sessionStorage.setItem('user', JSON.stringify(response.user));
                    sessionStorage.setItem('authToken', response.token || 'default-token');

                    console.log('[AuthContext] Login successful:', response.user.username);
                    return { success: true, user: response.user };
                } else {
                    throw new Error('Invalid username or password');
                }
            } else {
                // Development mode - mock login
                console.warn('[AuthContext] PyWebView API authenticate_user not available - using mock login');
                const mockUser = {
                    id: 1,
                    username: username,
                    email: username + '@sweetshopma.com',
                    role: 'admin',
                    first_name: 'Test',
                    last_name: 'User'
                };

                setUser(mockUser);
                setIsAuthenticated(true);
                sessionStorage.setItem('user', JSON.stringify(mockUser));
                sessionStorage.setItem('authToken', 'mock-token');

                return { success: true, user: mockUser };
            }
        } catch (err) {
            console.error('[AuthContext] Login error:', err);
            setError(err.message);
            return { success: false, error: err.message };
        } finally {
            setIsLoading(false);
        }
    };

    /**
     * Logout and clear session
     */
    const logout = async () => {
        try {
            setIsLoading(true);

            console.log('[AuthContext] Logging out user');

            // Call PyWebView API for logout if available
            if (window.pywebview && window.pywebview.api && window.pywebview.api.logout) {
                await window.pywebview.api.logout();
            }

            // Clear state
            setUser(null);
            setIsAuthenticated(false);
            setError(null);

            // Clear session storage
            sessionStorage.removeItem('user');
            sessionStorage.removeItem('authToken');

            console.log('[AuthContext] Logout successful');
        } catch (err) {
            console.error('[AuthContext] Logout error:', err);
            // Still clear local state even if API call fails
            setUser(null);
            setIsAuthenticated(false);
            sessionStorage.clear();
        } finally {
            setIsLoading(false);
        }
    };

    /**
     * Update user data
     */
    const updateUser = (userData) => {
        try {
            const updatedUser = { ...user, ...userData };
            setUser(updatedUser);
            sessionStorage.setItem('user', JSON.stringify(updatedUser));
            console.log('[AuthContext] User data updated');
        } catch (err) {
            console.error('[AuthContext] Error updating user:', err);
        }
    };

    const value = {
        user,
        isAuthenticated,
        isLoading,
        error,
        login,
        logout,
        updateUser,
        checkAuthStatus
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
}

AuthProvider.propTypes = {
    children: PropTypes.node.isRequired
};

export default AuthContext;
