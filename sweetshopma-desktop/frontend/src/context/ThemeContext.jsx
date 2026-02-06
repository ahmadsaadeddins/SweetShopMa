import React, { createContext, useContext, useState, useEffect } from 'react';
import * as PropTypes from 'prop-types';

/**
 * Theme Context
 * Provides theme management (light/dark mode) throughout the app
 */
const ThemeContext = createContext(null);

export const useTheme = () => {
    const context = useContext(ThemeContext);
    if (!context) {
        throw new Error('useTheme must be used within a ThemeProvider');
    }
    return context;
};

export function ThemeProvider({ children }) {
    const [theme, setTheme] = useState('light');
    const [isLoading, setIsLoading] = useState(true);

    // Load theme from localStorage on mount
    useEffect(() => {
        try {
            const savedTheme = localStorage.getItem('theme') || 'light';
            setTheme(savedTheme);
            applyTheme(savedTheme);
            console.log('[ThemeContext] Theme loaded:', savedTheme);
        } catch (err) {
            console.error('[ThemeContext] Error loading theme:', err);
        } finally {
            setIsLoading(false);
        }
    }, []);

    /**
     * Apply theme to document
     */
    const applyTheme = (themeName) => {
        if (typeof document !== 'undefined') {
            document.documentElement.setAttribute('data-theme', themeName);
        }
    };

    /**
     * Toggle between light and dark themes
     */
    const toggleTheme = () => {
        const newTheme = theme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);
        applyTheme(newTheme);

        try {
            localStorage.setItem('theme', newTheme);
            console.log('[ThemeContext] Theme toggled to:', newTheme);
        } catch (err) {
            console.error('[ThemeContext] Error saving theme:', err);
        }
    };

    /**
     * Set specific theme
     */
    const setThemeMode = (themeName) => {
        if (themeName !== 'light' && themeName !== 'dark') {
            console.warn('[ThemeContext] Invalid theme name:', themeName);
            return;
        }

        setTheme(themeName);
        applyTheme(themeName);

        try {
            localStorage.setItem('theme', themeName);
            console.log('[ThemeContext] Theme set to:', themeName);
        } catch (err) {
            console.error('[ThemeContext] Error saving theme:', err);
        }
    };

    const value = {
        theme,
        isLoading,
        toggleTheme,
        setTheme: setThemeMode,
        isLight: theme === 'light',
        isDark: theme === 'dark'
    };

    // Don't render children until theme is loaded
    if (isLoading) {
        return null;
    }

    return (
        <ThemeContext.Provider value={value}>
            {children}
        </ThemeContext.Provider>
    );
}

ThemeProvider.propTypes = {
    children: PropTypes.node.isRequired
};

export default ThemeContext;
