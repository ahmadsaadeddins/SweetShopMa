/**
 * Error Message Component
 * Displays an error message with optional retry button
 */

import React from 'react';

function ErrorMessage({ message, onRetry, style = {} }) {
    return (
        <div
            style={{
                padding: '16px',
                backgroundColor: '#fee2e2',
                border: '1px solid #fecaca',
                borderRadius: '8px',
                color: '#dc2626',
                ...style
            }}
        >
            <div style={{ fontWeight: 'bold', marginBottom: '8px' }}>
                Error
            </div>
            <div style={{ marginBottom: onRetry ? '12px' : 0 }}>
                {message}
            </div>
            {onRetry && (
                <button
                    onClick={onRetry}
                    style={{
                        padding: '8px 16px',
                        backgroundColor: '#dc2626',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer'
                    }}
                >
                    Retry
                </button>
            )}
        </div>
    );
}

export default ErrorMessage;
