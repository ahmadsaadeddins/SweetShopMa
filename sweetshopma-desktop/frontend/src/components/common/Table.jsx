import React from 'react';

/**
 * Reusable table component
 */
export function Table({
    columns = [],
    data = [],
    loading = false,
    emptyMessage = 'No data available',
    onRowClick = null,
    keyField = 'id',
    striped = true,
    hover = true,
    className = '',
}) {
    if (loading) {
        return (
            <div className="table-loading text-center py-5">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>
        );
    }

    if (!data || data.length === 0) {
        return (
            <div className="table-empty text-center py-5">
                <p className="text-muted mb-0">{emptyMessage}</p>
            </div>
        );
    }

    return (
        <div className={`table-responsive ${className}`}>
            <table className={`table ${striped ? 'table-striped' : ''} ${hover ? 'table-hover' : ''}`}>
                <thead>
                    <tr>
                        {columns.map((column, index) => (
                            <th
                                key={index}
                                style={{ width: column.width }}
                                className={column.className || ''}
                            >
                                {column.header}
                            </th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {data.map((row, rowIndex) => (
                        <tr
                            key={row[keyField] || rowIndex}
                            onClick={() => onRowClick && onRowClick(row)}
                            className={onRowClick ? 'table-row-clickable' : ''}
                        >
                            {columns.map((column, colIndex) => (
                                <td
                                    key={colIndex}
                                    className={column.cellClassName || ''}
                                >
                                    {column.render
                                        ? column.render(row[column.field], row, rowIndex)
                                        : row[column.field]}
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

/**
 * Simple table for displaying data without advanced features
 */
export function SimpleTable({
    headers = [],
    rows = [],
    loading = false,
    emptyMessage = 'No data available',
}) {
    if (loading) {
        return (
            <div className="text-center py-5">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>
        );
    }

    if (!rows || rows.length === 0) {
        return (
            <div className="text-center py-5">
                <p className="text-muted mb-0">{emptyMessage}</p>
            </div>
        );
    }

    return (
        <div className="table-responsive">
            <table className="table table-striped">
                <thead>
                    <tr>
                        {headers.map((header, index) => (
                            <th key={index}>{header}</th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {rows.map((row, rowIndex) => (
                        <tr key={rowIndex}>
                            {row.map((cell, cellIndex) => (
                                <td key={cellIndex}>{cell}</td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

/**
 * Data table with pagination
 */
export function DataTable({
    columns = [],
    data = [],
    loading = false,
    emptyMessage = 'No data available',
    onRowClick = null,
    keyField = 'id',
    pagination = null,
    onPageChange = null,
}) {
    if (loading) {
        return (
            <div className="text-center py-5">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>
        );
    }

    if (!data || data.length === 0) {
        return (
            <div className="text-center py-5">
                <p className="text-muted mb-0">{emptyMessage}</p>
            </div>
        );
    }

    return (
        <>
            <Table
                columns={columns}
                data={data}
                onRowClick={onRowClick}
                keyField={keyField}
            />
            {pagination && (
                <div className="d-flex justify-content-between align-items-center mt-3">
                    <small className="text-muted">
                        Showing {pagination.count} total records
                    </small>
                    <nav>
                        <ul className="pagination mb-0">
                            <li className={`page-item ${!pagination.previous ? 'disabled' : ''}`}>
                                <button
                                    className="page-link"
                                    onClick={() => onPageChange(pagination.previous)}
                                    disabled={!pagination.previous}
                                >
                                    Previous
                                </button>
                            </li>
                            <li className="page-item active">
                                <span className="page-link">
                                    Page {pagination.page} of {pagination.pages}
                                </span>
                            </li>
                            <li className={`page-item ${!pagination.next ? 'disabled' : ''}`}>
                                <button
                                    className="page-link"
                                    onClick={() => onPageChange(pagination.next)}
                                    disabled={!pagination.next}
                                >
                                    Next
                                </button>
                            </li>
                        </ul>
                    </nav>
                </div>
            )}
        </>
    );
}
