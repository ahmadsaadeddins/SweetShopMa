import React, { useState } from 'react';
import { Card, Button, Input, Checkbox, Alert } from '../components/common';
import { useSyncStatus } from '../hooks';
import { useApi } from '../services';
import { getStorageItem, setStorageItem, STORAGE_KEYS } from '../utils';

/**
 * SettingsPage Component
 * Application settings and configuration
 */
function SettingsPage() {
    const [activeTab, setActiveTab] = useState('general');
    const [saveMessage, setSaveMessage] = useState(null);

    // General settings
    const [shopName, setShopName] = useState(getStorageItem(STORAGE_KEYS.SHOP_NAME, 'Sweet Shop'));
    const [currency, setCurrency] = useState(getStorageItem(STORAGE_KEYS.CURRENCY, 'EGP'));
    const [taxRate, setTaxRate] = useState(getStorageItem(STORAGE_KEYS.TAX_RATE, '0'));

    // Display settings
    const [theme, setTheme] = useState(getStorageItem(STORAGE_KEYS.THEME, 'light'));
    const [lowStockAlerts, setLowStockAlerts] = useState(getStorageItem(STORAGE_KEYS.LOW_STOCK_ALERTS, true));

    // Sync settings
    const { data: syncStatus, refetch: refetchSync } = useSyncStatus();
    const { syncNow } = useApi();

    const handleSaveGeneral = () => {
        setStorageItem(STORAGE_KEYS.SHOP_NAME, shopName);
        setStorageItem(STORAGE_KEYS.CURRENCY, currency);
        setStorageItem(STORAGE_KEYS.TAX_RATE, taxRate);
        setSaveMessage({ type: 'success', text: 'General settings saved successfully!' });
        setTimeout(() => setSaveMessage(null), 3000);
    };

    const handleSaveDisplay = () => {
        setStorageItem(STORAGE_KEYS.THEME, theme);
        setStorageItem(STORAGE_KEYS.LOW_STOCK_ALERTS, lowStockAlerts);
        setSaveMessage({ type: 'success', text: 'Display settings saved successfully!' });
        setTimeout(() => setSaveMessage(null), 3000);
    };

    const handleSyncNow = async () => {
        try {
            const result = await syncNow();
            if (result.success) {
                setSaveMessage({ type: 'success', text: 'Sync completed successfully!' });
                refetchSync();
            } else {
                setSaveMessage({ type: 'danger', text: result.message || 'Sync failed' });
            }
            setTimeout(() => setSaveMessage(null), 3000);
        } catch (error) {
            setSaveMessage({ type: 'danger', text: `Sync error: ${error.message}` });
        }
    };

    const tabs = [
        { id: 'general', label: 'General', icon: '⚙️' },
        { id: 'display', label: 'Display', icon: '🎨' },
        { id: 'sync', label: 'Sync', icon: '🔄' },
        { id: 'about', label: 'About', icon: 'ℹ️' },
    ];

    return (
        <div className="p-4">
            {/* Header */}
            <div className="mb-4">
                <h1 className="h3 mb-1">Settings</h1>
                <p className="text-muted mb-0">Configure application preferences</p>
            </div>

            {/* Save Message */}
            {saveMessage && (
                <Alert
                    variant={saveMessage.type}
                    message={saveMessage.text}
                    dismissible
                    onDismiss={() => setSaveMessage(null)}
                />
            )}

            {/* Tabs */}
            <div className="mb-4">
                <ul className="nav nav-tabs">
                    {tabs.map(tab => (
                        <li className="nav-item" key={tab.id}>
                            <button
                                className={`nav-link ${activeTab === tab.id ? 'active' : ''}`}
                                onClick={() => setActiveTab(tab.id)}
                            >
                                {tab.icon} {tab.label}
                            </button>
                        </li>
                    ))}
                </ul>
            </div>

            {/* Tab Content */}
            <div className="tab-content">
                {/* General Settings */}
                {activeTab === 'general' && (
                    <Card title="General Settings">
                        <div className="row g-3">
                            <div className="col-12 col-md-6">
                                <Input
                                    label="Shop Name"
                                    value={shopName}
                                    onChange={(e) => setShopName(e.target.value)}
                                    placeholder="Enter shop name"
                                />
                            </div>
                            <div className="col-12 col-md-6">
                                <Input
                                    label="Currency"
                                    value={currency}
                                    onChange={(e) => setCurrency(e.target.value)}
                                    placeholder="e.g., EGP, USD"
                                />
                            </div>
                            <div className="col-12 col-md-6">
                                <Input
                                    label="Tax Rate (%)"
                                    type="number"
                                    value={taxRate}
                                    onChange={(e) => setTaxRate(e.target.value)}
                                    placeholder="0"
                                    min="0"
                                    max="100"
                                    step="0.01"
                                />
                            </div>
                        </div>
                        <div className="mt-4">
                            <Button variant="primary" onClick={handleSaveGeneral}>
                                Save Settings
                            </Button>
                        </div>
                    </Card>
                )}

                {/* Display Settings */}
                {activeTab === 'display' && (
                    <Card title="Display Settings">
                        <div className="row g-3">
                            <div className="col-12 col-md-6">
                                <label className="form-label">Theme</label>
                                <select
                                    className="form-select"
                                    value={theme}
                                    onChange={(e) => setTheme(e.target.value)}
                                >
                                    <option value="light">Light</option>
                                    <option value="dark">Dark</option>
                                    <option value="auto">Auto (System)</option>
                                </select>
                            </div>
                            <div className="col-12">
                                <Checkbox
                                    label="Enable low stock alerts"
                                    checked={lowStockAlerts}
                                    onChange={(e) => setLowStockAlerts(e.target.checked)}
                                    helpText="Show notifications when products are low on stock"
                                />
                            </div>
                        </div>
                        <div className="mt-4">
                            <Button variant="primary" onClick={handleSaveDisplay}>
                                Save Settings
                            </Button>
                        </div>
                    </Card>
                )}

                {/* Sync Settings */}
                {activeTab === 'sync' && (
                    <>
                        <Card title="Sync Status" className="mb-4">
                            {syncStatus ? (
                                <div className="row g-3">
                                    <div className="col-12 col-md-6">
                                        <strong>Status:</strong>{' '}
                                        <span className={`badge bg-${syncStatus.is_online ? 'success' : 'danger'}`}>
                                            {syncStatus.is_online ? 'Online' : 'Offline'}
                                        </span>
                                    </div>
                                    <div className="col-12 col-md-6">
                                        <strong>Syncing:</strong>{' '}
                                        {syncStatus.is_syncing ? (
                                            <span className="badge bg-warning">In Progress</span>
                                        ) : (
                                            <span className="badge bg-success">Idle</span>
                                        )}
                                    </div>
                                    <div className="col-12 col-md-6">
                                        <strong>Last Sync:</strong>{' '}
                                        {syncStatus.last_successful_sync
                                            ? new Date(syncStatus.last_successful_sync).toLocaleString()
                                            : 'Never'}
                                    </div>
                                    <div className="col-12 col-md-6">
                                        <strong>Sync Count:</strong> {syncStatus.sync_count || 0}
                                    </div>
                                    {syncStatus.last_error && (
                                        <div className="col-12">
                                            <strong>Last Error:</strong>{' '}
                                            <span className="text-danger">{syncStatus.last_error}</span>
                                        </div>
                                    )}
                                </div>
                            ) : (
                                <p className="text-muted mb-0">Sync service not available</p>
                            )}
                            <div className="mt-4">
                                <Button
                                    variant="primary"
                                    onClick={handleSyncNow}
                                    disabled={syncStatus?.is_syncing}
                                >
                                    {syncStatus?.is_syncing ? 'Syncing...' : 'Sync Now'}
                                </Button>
                            </div>
                        </Card>

                        <Card title="Sync Configuration">
                            <div className="row g-3">
                                <div className="col-12">
                                    <Checkbox
                                        label="Enable automatic sync"
                                        checked={syncStatus?.sync_interval > 0}
                                        helpText="Automatically sync data with the server at regular intervals"
                                        disabled
                                    />
                                </div>
                                <div className="col-12 col-md-6">
                                    <Input
                                        label="Sync Interval (minutes)"
                                        type="number"
                                        value={syncStatus?.sync_interval ? Math.round(syncStatus.sync_interval / 60) : 5}
                                        disabled
                                        helpText="How often to sync data automatically"
                                    />
                                </div>
                            </div>
                        </Card>
                    </>
                )}

                {/* About */}
                {activeTab === 'about' && (
                    <Card title="About SweetShopMa">
                        <div className="text-center mb-4">
                            <h2 className="h3">SweetShopMa Desktop</h2>
                            <p className="text-muted">Version 1.0.0</p>
                        </div>
                        <hr />
                        <div className="row g-3">
                            <div className="col-12">
                                <h5 className="mb-3">Application Information</h5>
                                <dl className="row">
                                    <dt className="col-sm-4">Version:</dt>
                                    <dd className="col-sm-8">1.0.0</dd>

                                    <dt className="col-sm-4">Build:</dt>
                                    <dd className="col-sm-8">2025.02.03</dd>

                                    <dt className="col-sm-4">Platform:</dt>
                                    <dd className="col-sm-8">
                                        {navigator.platform || 'Unknown'}
                                    </dd>

                                    <dt className="col-sm-4">User Agent:</dt>
                                    <dd className="col-sm-8">
                                        <small className="text-muted">
                                            {navigator.userAgent}
                                        </small>
                                    </dd>
                                </dl>
                            </div>
                            <div className="col-12">
                                <h5 className="mb-3">Technologies</h5>
                                <ul className="list-unstyled">
                                    <li>⚛️ React 18.2.0</li>
                                    <li>🎨 React Router DOM 6.20.0</li>
                                    <li>⚡ Vite 5.0.0</li>
                                    <li>🐍 Django REST Framework</li>
                                    <li>🪟 PyWebView</li>
                                </ul>
                            </div>
                            <div className="col-12">
                                <h5 className="mb-3">License</h5>
                                <p className="text-muted mb-0">
                                    Copyright © 2025 SweetShopMa. All rights reserved.
                                </p>
                            </div>
                        </div>
                    </Card>
                )}
            </div>
        </div>
    );
}

export default SettingsPage;
