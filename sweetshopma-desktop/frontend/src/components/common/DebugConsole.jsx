/**
 * DebugConsole Component
 * Visual debugging panel for development and troubleshooting
 * Shows console logs, API calls, and errors with copy/clear functionality
 */

import React, { useState, useEffect, useRef } from 'react';

function DebugConsole({ position = 'bottom-right' }) {
    const [logs, setLogs] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [maxLogs, setMaxLogs] = useState(100);
    const logContainerRef = useRef(null);

    // Position styles
    const positionStyles = {
        'bottom-right': {
            position: 'fixed',
            bottom: '20px',
            right: '20px',
            zIndex: 9999
        },
        'top-right': {
            position: 'fixed',
            top: '20px',
            right: '20px',
            zIndex: 9999
        },
        'top-left': {
            position: 'fixed',
            top: '20px',
            left: '20px',
            zIndex: 9999
        }
    };

    const selectedPosition = positionStyles[position] || positionStyles['bottom-right'];

    const logCounter = useRef(0);

    // Add log entry
    const addLog = (message, type = 'info', data = null) => {
        const timestamp = new Date().toLocaleTimeString();
        logCounter.current += 1;
        const logEntry = {
            id: `${Date.now()}-${logCounter.current}-${Math.random().toString(36).substr(2, 9)}`,
            timestamp,
            message,
            type,
            data
        };

        setLogs(prevLogs => {
            const newLogs = [...prevLogs, logEntry];
            // Keep only the last maxLogs entries
            return newLogs.slice(-maxLogs);
        });
    };

    // Intercept console methods
    useEffect(() => {
        const originalLog = console.log;
        const originalWarn = console.warn;
        const originalError = console.error;
        const originalInfo = console.info;

        console.log = (...args) => {
            originalLog(...args);
            setTimeout(() => addLog(args.join(' '), 'log'), 0);
        };

        console.warn = (...args) => {
            originalWarn(...args);
            setTimeout(() => addLog(args.join(' '), 'warning'), 0);
        };

        console.error = (...args) => {
            originalError(...args);
            setTimeout(() => addLog(args.join(' '), 'error'), 0);
        };

        console.info = (...args) => {
            originalInfo(...args);
            setTimeout(() => addLog(args.join(' '), 'info'), 0);
        };

        // Log initialization
        setTimeout(() => addLog('🔧 DebugConsole initialized', 'info'), 0);

        return () => {
            // Restore original console methods
            console.log = originalLog;
            console.warn = originalWarn;
            console.error = originalError;
            console.info = originalInfo;
        };
    }, [maxLogs]);

    // Auto-scroll to bottom when new logs arrive
    useEffect(() => {
        if (logContainerRef.current && isOpen) {
            logContainerRef.current.scrollTop = logContainerRef.current.scrollHeight;
        }
    }, [logs, isOpen]);

    // Clear logs
    const handleClear = () => {
        setLogs([]);
        addLog('🗑️ Logs cleared', 'info');
    };

    // Copy logs to clipboard
    const handleCopy = () => {
        const logText = logs.map(log => {
            const dataStr = log.data ? `\n  Data: ${JSON.stringify(log.data, null, 2)}` : '';
            return `[${log.timestamp}] [${log.type.toUpperCase()}] ${log.message}${dataStr}`;
        }).join('\n');

        navigator.clipboard.writeText(logText).then(() => {
            addLog('📋 Logs copied to clipboard', 'success');
        }).catch(err => {
            addLog(`❌ Failed to copy logs: ${err.message}`, 'error');
        });
    };

    // Get log type color
    const getLogColor = (type) => {
        const colors = {
            'log': '#00ff00',
            'info': '#00bfff',
            'warning': '#ffa500',
            'error': '#ff4444',
            'success': '#00ff00'
        };
        return colors[type] || '#ffffff';
    };

    // Get log icon
    const getLogIcon = (type) => {
        const icons = {
            'log': '📝',
            'info': 'ℹ️',
            'warning': '⚠️',
            'error': '❌',
            'success': '✅'
        };
        return icons[type] || '•';
    };

    return (
        <div style={selectedPosition}>
            {/* Toggle Button */}
            <button
                onClick={() => setIsOpen(!isOpen)}
                style={{
                    backgroundColor: isOpen ? '#ff4444' : '#4CAF50',
                    color: 'white',
                    border: 'none',
                    borderRadius: '5px',
                    padding: '10px 15px',
                    cursor: 'pointer',
                    fontSize: '14px',
                    fontWeight: 'bold',
                    boxShadow: '0 2px 5px rgba(0,0,0,0.3)',
                    marginBottom: isOpen ? '10px' : '0'
                }}
            >
                {isOpen ? '✕ Close Debug' : '🔧 Debug Console'}
                {logs.length > 0 && (
                    <span style={{
                        backgroundColor: 'rgba(255,255,255,0.3)',
                        borderRadius: '10px',
                        padding: '2px 8px',
                        marginLeft: '8px',
                        fontSize: '12px'
                    }}>
                        {logs.length}
                    </span>
                )}
            </button>

            {/* Console Panel */}
            {isOpen && (
                <div style={{
                    backgroundColor: '#1e1e1e',
                    border: '2px solid #444',
                    borderRadius: '5px',
                    boxShadow: '0 4px 10px rgba(0,0,0,0.5)',
                    width: '600px',
                    maxHeight: '400px',
                    display: 'flex',
                    flexDirection: 'column'
                }}>
                    {/* Header */}
                    <div style={{
                        backgroundColor: '#2d2d2d',
                        padding: '10px',
                        borderBottom: '1px solid #444',
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center'
                    }}>
                        <span style={{
                            color: '#fff',
                            fontWeight: 'bold',
                            fontSize: '14px'
                        }}>
                            🔧 Debug Console
                        </span>
                        <div>
                            <button
                                onClick={handleCopy}
                                style={{
                                    backgroundColor: '#4CAF50',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '3px',
                                    padding: '5px 10px',
                                    cursor: 'pointer',
                                    fontSize: '12px',
                                    marginRight: '5px'
                                }}
                            >
                                📋 Copy
                            </button>
                            <button
                                onClick={handleClear}
                                style={{
                                    backgroundColor: '#f44336',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '3px',
                                    padding: '5px 10px',
                                    cursor: 'pointer',
                                    fontSize: '12px'
                                }}
                            >
                                🗑️ Clear
                            </button>
                        </div>
                    </div>

                    {/* Log Entries */}
                    <div
                        ref={logContainerRef}
                        style={{
                            padding: '10px',
                            overflowY: 'auto',
                            flex: 1,
                            fontFamily: 'Courier New, monospace',
                            fontSize: '12px',
                            lineHeight: '1.4'
                        }}
                    >
                        {logs.length === 0 ? (
                            <div style={{ color: '#888', textAlign: 'center', padding: '20px' }}>
                                No logs yet...
                            </div>
                        ) : (
                            logs.map(log => (
                                <div
                                    key={log.id}
                                    style={{
                                        marginBottom: '5px',
                                        paddingBottom: '5px',
                                        borderBottom: '1px solid #333',
                                        color: getLogColor(log.type)
                                    }}
                                >
                                    <span style={{ color: '#888' }}>
                                        [{log.timestamp}]
                                    </span>
                                    <span style={{ marginLeft: '8px' }}>
                                        {getLogIcon(log.type)}
                                    </span>
                                    <span style={{ marginLeft: '5px' }}>
                                        {log.message}
                                    </span>
                                    {log.data && (
                                        <pre style={{
                                            marginLeft: '20px',
                                            marginTop: '5px',
                                            color: '#ffff00',
                                            fontSize: '11px',
                                            whiteSpace: 'pre-wrap',
                                            wordBreak: 'break-word'
                                        }}>
                                            {JSON.stringify(log.data, null, 2)}
                                        </pre>
                                    )}
                                </div>
                            ))
                        )}
                    </div>

                    {/* Footer */}
                    <div style={{
                        backgroundColor: '#2d2d2d',
                        padding: '5px 10px',
                        borderTop: '1px solid #444',
                        fontSize: '11px',
                        color: '#888',
                        display: 'flex',
                        justifyContent: 'space-between'
                    }}>
                        <span>{logs.length} log entries</span>
                        <label>
                            Max:
                            <input
                                type="number"
                                value={maxLogs}
                                onChange={(e) => setMaxLogs(Number(e.target.value))}
                                style={{
                                    width: '50px',
                                    marginLeft: '5px',
                                    backgroundColor: '#1e1e1e',
                                    border: '1px solid #444',
                                    color: '#fff',
                                    borderRadius: '3px',
                                    padding: '2px 5px'
                                }}
                            />
                        </label>
                    </div>
                </div>
            )}
        </div>
    );
}

export default DebugConsole;
