import React from 'react';

/**
 * Reusable input component with label and error handling
 */
export function Input({
    label,
    type = 'text',
    value,
    onChange,
    placeholder = '',
    error = null,
    disabled = false,
    required = false,
    className = '',
    helpText = null,
    ...props
}) {
    const inputId = `input-${Math.random().toString(36).substr(2, 9)}`;

    return (
        <div className={`form-group ${className}`}>
            {label && (
                <label htmlFor={inputId} className="form-label">
                    {label}
                    {required && <span className="text-danger ms-1">*</span>}
                </label>
            )}
            <input
                id={inputId}
                type={type}
                value={value}
                onChange={onChange}
                placeholder={placeholder}
                disabled={disabled}
                required={required}
                className={`form-control ${error ? 'is-invalid' : ''}`}
                {...props}
            />
            {error && (
                <div className="invalid-feedback">{error}</div>
            )}
            {helpText && !error && (
                <small className="form-text text-muted">{helpText}</small>
            )}
        </div>
    );
}

/**
 * Select input component
 */
export function Select({
    label,
    value,
    onChange,
    options = [],
    placeholder = 'Select...',
    error = null,
    disabled = false,
    required = false,
    className = '',
    helpText = null,
    ...props
}) {
    const inputId = `select-${Math.random().toString(36).substr(2, 9)}`;

    return (
        <div className={`form-group ${className}`}>
            {label && (
                <label htmlFor={inputId} className="form-label">
                    {label}
                    {required && <span className="text-danger ms-1">*</span>}
                </label>
            )}
            <select
                id={inputId}
                value={value}
                onChange={onChange}
                disabled={disabled}
                required={required}
                className={`form-select ${error ? 'is-invalid' : ''}`}
                {...props}
            >
                {placeholder && (
                    <option value="">{placeholder}</option>
                )}
                {options.map((option) => (
                    <option key={option.value} value={option.value}>
                        {option.label}
                    </option>
                ))}
            </select>
            {error && (
                <div className="invalid-feedback">{error}</div>
            )}
            {helpText && !error && (
                <small className="form-text text-muted">{helpText}</small>
            )}
        </div>
    );
}

/**
 * Textarea component
 */
export function Textarea({
    label,
    value,
    onChange,
    placeholder = '',
    error = null,
    disabled = false,
    required = false,
    rows = 3,
    className = '',
    helpText = null,
    ...props
}) {
    const inputId = `textarea-${Math.random().toString(36).substr(2, 9)}`;

    return (
        <div className={`form-group ${className}`}>
            {label && (
                <label htmlFor={inputId} className="form-label">
                    {label}
                    {required && <span className="text-danger ms-1">*</span>}
                </label>
            )}
            <textarea
                id={inputId}
                value={value}
                onChange={onChange}
                placeholder={placeholder}
                disabled={disabled}
                required={required}
                rows={rows}
                className={`form-control ${error ? 'is-invalid' : ''}`}
                {...props}
            />
            {error && (
                <div className="invalid-feedback">{error}</div>
            )}
            {helpText && !error && (
                <small className="form-text text-muted">{helpText}</small>
            )}
        </div>
    );
}

/**
 * Checkbox component
 */
export function Checkbox({
    label,
    checked,
    onChange,
    disabled = false,
    className = '',
    helpText = null,
    ...props
}) {
    const inputId = `checkbox-${Math.random().toString(36).substr(2, 9)}`;

    return (
        <div className={`form-check ${className}`}>
            <input
                id={inputId}
                type="checkbox"
                checked={checked}
                onChange={onChange}
                disabled={disabled}
                className="form-check-input"
                {...props}
            />
            {label && (
                <label htmlFor={inputId} className="form-check-label">
                    {label}
                </label>
            )}
            {helpText && (
                <small className="form-text text-muted d-block">{helpText}</small>
            )}
        </div>
    );
}
