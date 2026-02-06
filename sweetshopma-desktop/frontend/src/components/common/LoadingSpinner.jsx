import React from 'react';
import * as PropTypes from 'prop-types';

/**
 * LoadingSpinner Component
 * Displays a loading spinner with optional message
 */
function LoadingSpinner({ size = 'md', message = 'Loading...', fullScreen = false }) {
    const sizeMap = {
        sm: '24px',
        md: '40px',
        lg: '64px',
        xl: '80px'
    };

    const spinnerSize = sizeMap[size] || sizeMap.md;

    const containerStyle = fullScreen ? {
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'rgba(255, 255, 255, 0.9)',
        zIndex: 9999
    } : {
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '24px'
    };

    const spinnerStyle = {
        width: spinnerSize,
        height: spinnerSize,
        border: '4px solid var(--color-gray-200)',
        borderTop: `4px solid var(--color-primary-600)`,
        borderRadius: '50%',
        animation: 'spin 1s linear infinite'
    };

    const messageStyle = {
        marginTop: '16px',
        fontSize: '14px',
        color: 'var(--color-text-secondary)',
        textAlign: 'center'
    };

    return (
        <div style={containerStyle}>
            <div style={spinnerStyle} />
            {message && <p style={messageStyle}>{message}</p>}

            <style jsx>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
        </div>
    );
}

LoadingSpinner.propTypes = {
    size: PropTypes.oneOf(['sm', 'md', 'lg', 'xl']),
    message: PropTypes.string,
    fullScreen: PropTypes.bool
};

export default LoadingSpinner;
