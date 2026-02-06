// Import React
import React from 'react';
import ReactDOM from 'react-dom/client';

// Import global styles
import './styles/global.css';
import './styles/variables.css';

// Import root App component
import App from './App';

// Import error boundary
import ErrorBoundary from './components/common/ErrorBoundary';

// Log initialization
console.log('[SweetShopMa] Initializing React application...');
console.log('[SweetShopMa] Environment:', process.env.NODE_ENV || 'development');

// Check if pywebview API is available
function checkPyWebViewAPI() {
    if (window.pywebview && window.pywebview.api) {
        console.log('[SweetShopMa] ✓ PyWebView API detected');
        return true;
    } else {
        console.warn('[SweetShopMa] ⚠ PyWebView API not detected - running in browser mode');
        return false;
    }
}

// Initialize the app
function initApp() {
    const rootElement = document.getElementById('root');

    if (!rootElement) {
        console.error('[SweetShopMa] ✗ Root element not found!');
        return;
    }

    // Check PyWebView API
    const hasPyWebView = checkPyWebViewAPI();

    // Create React root
    const root = ReactDOM.createRoot(rootElement);

    // Render app with error boundary
    root.render(
        <ErrorBoundary>
            <App hasPyWebView={hasPyWebView} />
        </ErrorBoundary>
    );

    console.log('[SweetShopMa] ✓ Application initialized successfully');
}

// Wait for DOM to be ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initApp);
} else {
    initApp();
}

// Handle hot module replacement in development
if (typeof module !== 'undefined' && module.hot) {
    module.hot.accept();
}
