import React from 'react';

/**
 * Card component for grouping related content
 */
export function Card({
    children,
    title = null,
    subtitle = null,
    actions = null,
    className = '',
    padding = true,
    variant = 'default',
}) {
    const variantClasses = {
        default: 'card',
        primary: 'card card-primary',
        success: 'card card-success',
        warning: 'card card-warning',
        danger: 'card card-danger',
        info: 'card card-info',
    };

    return (
        <div className={`${variantClasses[variant]} ${className}`}>
            {(title || subtitle || actions) && (
                <div className="card-header">
                    <div className="d-flex justify-content-between align-items-center">
                        <div>
                            {title && <h5 className="card-title mb-0">{title}</h5>}
                            {subtitle && <p className="card-subtitle mb-0">{subtitle}</p>}
                        </div>
                        {actions && <div className="card-actions">{actions}</div>}
                    </div>
                </div>
            )}
            <div className={padding ? 'card-body' : 'card-body p-0'}>
                {children}
            </div>
        </div>
    );
}

/**
 * Stat card component for displaying metrics
 */
export function StatCard({
    title,
    value,
    subtitle = null,
    icon = null,
    trend = null,
    variant = 'primary',
    loading = false,
}) {
    const variantClasses = {
        primary: 'text-primary',
        success: 'text-success',
        danger: 'text-danger',
        warning: 'text-warning',
        info: 'text-info',
    };

    const bgClasses = {
        primary: 'bg-primary',
        success: 'bg-success',
        danger: 'bg-danger',
        warning: 'bg-warning',
        info: 'bg-info',
    };

    if (loading) {
        return (
            <Card className="stat-card">
                <div className="d-flex align-items-center">
                    <div className="flex-grow-1">
                        <div className="placeholder-glow">
                            <span className="placeholder col-6"></span>
                            <span className="placeholder col-8 placeholder-lg"></span>
                        </div>
                    </div>
                </div>
            </Card>
        );
    }

    return (
        <Card className={`stat-card stat-card-${variant}`}>
            <div className="d-flex align-items-center">
                {icon && (
                    <div className={`stat-icon ${bgClasses[variant]} me-3`}>
                        {icon}
                    </div>
                )}
                <div className="flex-grow-1">
                    <h6 className="stat-title text-muted mb-1">{title}</h6>
                    <div className={`stat-value ${variantClasses[variant]} mb-1`}>
                        {value}
                    </div>
                    {subtitle && (
                        <small className="stat-subtitle text-muted">{subtitle}</small>
                    )}
                    {trend && (
                        <div className={`stat-trend ${trend > 0 ? 'text-success' : 'text-danger'}`}>
                            {trend > 0 ? '↑' : '↓'} {Math.abs(trend)}%
                        </div>
                    )}
                </div>
            </div>
        </Card>
    );
}

/**
 * Alert card component for displaying messages
 */
export function Alert({
    variant = 'info',
    message,
    dismissible = false,
    onDismiss = null,
}) {
    const variantClasses = {
        primary: 'alert-primary',
        success: 'alert-success',
        danger: 'alert-danger',
        warning: 'alert-warning',
        info: 'alert-info',
    };

    return (
        <div className={`alert ${variantClasses[variant]} ${dismissible ? 'alert-dismissible' : ''}`} role="alert">
            {message}
            {dismissible && (
                <button
                    type="button"
                    className="btn-close"
                    onClick={onDismiss}
                    aria-label="Close"
                ></button>
            )}
        </div>
    );
}
