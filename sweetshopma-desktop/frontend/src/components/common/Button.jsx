import React from 'react';

/**
 * Reusable button component with variants
 */
export function Button({
    children,
    variant = 'primary',
    size = 'medium',
    disabled = false,
    loading = false,
    onClick,
    type = 'button',
    className = '',
    ...props
}) {
    const baseClasses = 'btn';

    const variantClasses = {
        primary: 'btn-primary',
        secondary: 'btn-secondary',
        success: 'btn-success',
        danger: 'btn-danger',
        warning: 'btn-warning',
        ghost: 'btn-ghost',
    };

    const sizeClasses = {
        small: 'btn-sm',
        medium: '',
        large: 'btn-lg',
    };

    const classes = [
        baseClasses,
        variantClasses[variant],
        sizeClasses[size],
        className,
    ].filter(Boolean).join(' ');

    return (
        <button
            type={type}
            className={classes}
            disabled={disabled || loading}
            onClick={onClick}
            {...props}
        >
            {loading ? (
                <>
                    <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    Loading...
                </>
            ) : children}
        </button>
    );
}
