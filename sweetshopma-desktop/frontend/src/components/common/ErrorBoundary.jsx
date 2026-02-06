import React from 'react';
import * as PropTypes from 'prop-types';

/**
 * ErrorBoundary Component
 * Catches JavaScript errors anywhere in the component tree,
 * logs those errors, and displays a fallback UI
 */
class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            hasError: false,
            error: null,
            errorInfo: null
        };
    }

    static getDerivedStateFromError(error) {
        // Update state so the next render will show the fallback UI
        return { hasError: true };
    }

    componentDidCatch(error, errorInfo) {
        // Log the error to the console
        console.error('[ErrorBoundary] Caught an error:', error, errorInfo);

        // Log additional component stack
        console.error('[ErrorBoundary] Component stack:', errorInfo.componentStack);

        // Store error info in state
        this.setState({
            error: error,
            errorInfo: errorInfo
        });
    }

    handleReset = () => {
        this.setState({
            hasError: false,
            error: null,
            errorInfo: null
        });
    };

    render() {
        if (this.state.hasError) {
            // Custom fallback UI
            return (
                <div style={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    minHeight: '100vh',
                    padding: '24px',
                    backgroundColor: 'var(--color-error-50)',
                    color: 'var(--color-error-900)'
                }}>
                    <div style={{
                        maxWidth: '600px',
                        width: '100%',
                        backgroundColor: 'white',
                        borderRadius: '8px',
                        padding: '32px',
                        boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)'
                    }}>
                        <h1 style={{
                            fontSize: '24px',
                            marginBottom: '16px',
                            color: 'var(--color-error-600)'
                        }}>
                            Something went wrong
                        </h1>

                        <p style={{
                            fontSize: '16px',
                            marginBottom: '24px',
                            color: 'var(--color-gray-700)'
                        }}>
                            An unexpected error occurred. Please try refreshing the page or contact support if the problem persists.
                        </p>

                        {this.state.error && (
                            <details style={{
                                marginBottom: '24px',
                                padding: '16px',
                                backgroundColor: 'var(--color-gray-100)',
                                borderRadius: '4px',
                                fontSize: '14px'
                            }}>
                                <summary style={{
                                    cursor: 'pointer',
                                    fontWeight: '600',
                                    marginBottom: '8px'
                                }}>
                                    Error Details
                                </summary>
                                <pre style={{
                                    whiteSpace: 'pre-wrap',
                                    wordBreak: 'break-word',
                                    margin: 0,
                                    fontSize: '12px',
                                    color: 'var(--color-error-700)'
                                }}>
                                    {this.state.error.toString()}
                                    {this.state.errorInfo && this.state.errorInfo.componentStack}
                                </pre>
                            </details>
                        )}

                        <div style={{
                            display: 'flex',
                            gap: '12px',
                            justifyContent: 'flex-end'
                        }}>
                            <button
                                onClick={this.handleReset}
                                style={{
                                    padding: '10px 20px',
                                    backgroundColor: 'var(--color-primary-600)',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    fontSize: '14px',
                                    fontWeight: '500',
                                    cursor: 'pointer',
                                    transition: 'background-color 0.2s'
                                }}
                                onMouseOver={(e) => {
                                    e.target.style.backgroundColor = 'var(--color-primary-700)';
                                }}
                                onMouseOut={(e) => {
                                    e.target.style.backgroundColor = 'var(--color-primary-600)';
                                }}
                            >
                                Try Again
                            </button>

                            <button
                                onClick={() => window.location.reload()}
                                style={{
                                    padding: '10px 20px',
                                    backgroundColor: 'var(--color-gray-200)',
                                    color: 'var(--color-gray-700)',
                                    border: 'none',
                                    borderRadius: '4px',
                                    fontSize: '14px',
                                    fontWeight: '500',
                                    cursor: 'pointer',
                                    transition: 'background-color 0.2s'
                                }}
                                onMouseOver={(e) => {
                                    e.target.style.backgroundColor = 'var(--color-gray-300)';
                                }}
                                onMouseOut={(e) => {
                                    e.target.style.backgroundColor = 'var(--color-gray-200)';
                                }}
                            >
                                Reload Page
                            </button>
                        </div>
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}

ErrorBoundary.propTypes = {
    children: PropTypes.node.isRequired
};

export default ErrorBoundary;
