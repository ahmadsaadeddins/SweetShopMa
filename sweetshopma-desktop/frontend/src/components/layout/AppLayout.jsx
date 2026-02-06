import React from 'react';
import { Outlet } from 'react-router-dom';
import Header from './Header';
import Sidebar from './Sidebar';
import { DebugConsole } from '../common';
import { useApiContext } from '../../context/ApiContext';
import { useSidebar } from '../../context/SidebarContext';
import { useAuth } from '../../context/AuthContext';

/**
 * AppLayout Component
 * Main layout wrapper for authenticated pages
 * Includes header, sidebar, main content area, and API initialization loading
 */
function AppLayout() {
    const { isReady, isPolling } = useApiContext();
    const { isCollapsed } = useSidebar();
    const { user } = useAuth();

    // Check if user is Seller - hide sidebar for Sellers
    const isSeller = user?.role === 'Seller';

    const layoutStyle = {
        display: 'flex',
        flexDirection: 'column',
        minHeight: '100vh',
        backgroundColor: 'var(--color-background)',
        position: 'relative'
    };

    const contentStyle = {
        display: 'flex',
        flex: 1,
        overflow: 'hidden'
    };

    const mainStyle = {
        flex: 1,
        overflow: 'auto',
        padding: '24px',
        backgroundColor: 'var(--color-background)',
        marginLeft: isSeller ? '0' : (isCollapsed ? '72px' : '240px'),
        transition: 'margin-left var(--transition-normal)',
        width: isSeller ? '100%' : 'auto'
    };

    const loadingOverlayStyle = {
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(255, 255, 255, 0.9)',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 9999,
        backdropFilter: 'blur(4px)'
    };

    const loadingSpinnerStyle = {
        width: '48px',
        height: '48px',
        border: '4px solid var(--color-border, #e0e0e0)',
        borderTopColor: 'var(--color-primary, #0066cc)',
        borderRadius: '50%',
        animation: 'spin 1s linear infinite'
    };

    const loadingTextStyle = {
        marginTop: '16px',
        fontSize: '16px',
        color: 'var(--color-text, #333)',
        fontWeight: 500
    };

    // Show loading overlay when API is initializing
    const showLoading = isPolling;

    return (
        <div style={layoutStyle}>
            {/* API Initialization Loading Overlay */}
            {showLoading && (
                <div style={loadingOverlayStyle}>
                    <style>
                        {`@keyframes spin {
                            to { transform: rotate(360deg); }
                        }`}
                    </style>
                    <div style={loadingSpinnerStyle} />
                    <div style={loadingTextStyle}>Initializing API...</div>
                </div>
            )}

            {/* Header */}
            <Header />

            {/* Main content area with sidebar */}
            <div style={contentStyle}>
                <Sidebar />
                <main style={mainStyle}>
                    <Outlet />
                </main>
            </div>

            {/* Debug Console */}
            <DebugConsole position="bottom-right" />
        </div>
    );
}

export default AppLayout;
