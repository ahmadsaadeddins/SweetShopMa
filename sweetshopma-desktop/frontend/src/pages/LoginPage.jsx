import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/**
 * LoginPage Component
 * User authentication page with "Remember Me" functionality
 */
function LoginPage() {
    const navigate = useNavigate();
    const { login, isLoading } = useAuth();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [rememberMe, setRememberMe] = useState(false);
    const [error, setError] = useState('');
    const [credentialsLoaded, setCredentialsLoaded] = useState(false);

    // Load saved credentials on mount via PyWebView API
    useEffect(() => {
        const loadCredentials = async () => {
            try {
                // Check if PyWebView API is available
                if (window.pywebview && window.pywebview.api &&
                    typeof window.pywebview.api.get_saved_credentials === 'function') {
                    const saved = await window.pywebview.api.get_saved_credentials();
                    if (saved && saved.username) {
                        setUsername(saved.username || '');
                        setPassword(saved.password || '');
                        setRememberMe(true);
                        console.log('[LoginPage] Loaded saved credentials');
                    }
                }
            } catch (err) {
                console.error('[LoginPage] Error loading saved credentials:', err);
            } finally {
                setCredentialsLoaded(true);
            }
        };

        // Wait a bit for PyWebView to be ready
        const timer = setTimeout(loadCredentials, 500);
        return () => clearTimeout(timer);
    }, []);

    const containerStyle = {
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'var(--color-gray-100)',
        padding: '24px'
    };

    const cardStyle = {
        width: '100%',
        maxWidth: '400px',
        backgroundColor: 'white',
        borderRadius: 'var(--radius-lg)',
        padding: '40px',
        boxShadow: 'var(--shadow-lg)'
    };

    const headerStyle = {
        textAlign: 'center',
        marginBottom: '32px'
    };

    const titleStyle = {
        fontSize: '28px',
        fontWeight: '700',
        color: 'var(--color-primary-600)',
        marginBottom: '8px'
    };

    const subtitleStyle = {
        fontSize: '14px',
        color: 'var(--color-text-secondary)'
    };

    const formStyle = {
        display: 'flex',
        flexDirection: 'column',
        gap: '20px'
    };

    const inputGroupStyle = {
        display: 'flex',
        flexDirection: 'column',
        gap: '8px'
    };

    const labelStyle = {
        fontSize: '14px',
        fontWeight: '500',
        color: 'var(--color-text-primary)'
    };

    const inputStyle = {
        padding: '12px 16px',
        fontSize: '14px',
        border: '1px solid var(--color-border)',
        borderRadius: 'var(--radius-md)',
        outline: 'none',
        transition: 'border-color var(--transition-fast)'
    };

    const checkboxGroupStyle = {
        display: 'flex',
        alignItems: 'center',
        gap: '8px'
    };

    const checkboxStyle = {
        width: '18px',
        height: '18px',
        cursor: 'pointer',
        accentColor: 'var(--color-primary-600)'
    };

    const checkboxLabelStyle = {
        fontSize: '14px',
        color: 'var(--color-text-secondary)',
        cursor: 'pointer',
        userSelect: 'none'
    };

    const buttonStyle = {
        padding: '12px 24px',
        fontSize: '16px',
        fontWeight: '600',
        color: 'white',
        backgroundColor: 'var(--color-primary-600)',
        border: 'none',
        borderRadius: 'var(--radius-md)',
        cursor: 'pointer',
        transition: 'background-color var(--transition-fast)',
        opacity: isLoading ? 0.6 : 1
    };

    const errorStyle = {
        padding: '12px',
        backgroundColor: 'var(--color-error-50)',
        color: 'var(--color-error-700)',
        borderRadius: 'var(--radius-md)',
        fontSize: '14px',
        border: '1px solid var(--color-error-200)'
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (!username || !password) {
            setError('Please enter both username and password');
            return;
        }

        // Save or clear credentials based on checkbox via PyWebView API
        if (window.pywebview && window.pywebview.api) {
            try {
                if (rememberMe) {
                    await window.pywebview.api.save_credentials(username, password);
                } else {
                    await window.pywebview.api.clear_saved_credentials();
                }
            } catch (err) {
                console.error('[LoginPage] Error saving credentials:', err);
            }
        }

        const result = await login(username, password);

        if (result.success) {
            // DEBUG: Log user role for debugging
            console.log('[LoginPage] Login successful, user role:', result.user?.role);
            console.log('[LoginPage] Full user object:', result.user);

            // Redirect based on role
            const userRole = result.user?.role || 'User';
            console.log('[LoginPage] Redirecting to:', userRole === 'Seller' ? '/pos' : '/dashboard');

            if (userRole === 'Seller') {
                navigate('/pos');  // Sellers go directly to POS
            } else {
                navigate('/dashboard');  // Others go to dashboard
            }
        } else {
            setError(result.error || 'Login failed. Please try again.');
        }
    };

    const handleRememberMeChange = async (e) => {
        const checked = e.target.checked;
        setRememberMe(checked);

        // If unchecking, clear saved credentials
        if (!checked && window.pywebview && window.pywebview.api) {
            try {
                await window.pywebview.api.clear_saved_credentials();
            } catch (err) {
                console.error('[LoginPage] Error clearing credentials:', err);
            }
        }
    };

    return (
        <div style={containerStyle}>
            <div style={cardStyle}>
                <div style={headerStyle}>
                    <h1 style={titleStyle}>SweetShopMa</h1>
                    <p style={subtitleStyle}>Sign in to your account</p>
                </div>

                {error && <div style={errorStyle}>{error}</div>}

                <form style={formStyle} onSubmit={handleSubmit}>
                    <div style={inputGroupStyle}>
                        <label htmlFor="username" style={labelStyle}>
                            Username
                        </label>
                        <input
                            id="username"
                            type="text"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            style={inputStyle}
                            placeholder="Enter your username"
                            disabled={isLoading}
                            autoFocus
                        />
                    </div>

                    <div style={inputGroupStyle}>
                        <label htmlFor="password" style={labelStyle}>
                            Password
                        </label>
                        <input
                            id="password"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            style={inputStyle}
                            placeholder="Enter your password"
                            disabled={isLoading}
                        />
                    </div>

                    <div style={checkboxGroupStyle}>
                        <input
                            id="rememberMe"
                            type="checkbox"
                            checked={rememberMe}
                            onChange={handleRememberMeChange}
                            style={checkboxStyle}
                            disabled={isLoading}
                        />
                        <label htmlFor="rememberMe" style={checkboxLabelStyle}>
                            Remember me
                        </label>
                    </div>

                    <button
                        type="submit"
                        style={buttonStyle}
                        disabled={isLoading}
                    >
                        {isLoading ? 'Signing in...' : 'Sign In'}
                    </button>
                </form>
            </div>
        </div>
    );
}

export default LoginPage;
